using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.Services;
using NLog;
using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace GitHub.Services
{
    [Export(typeof(ISynchronizeForkService))]
    public class SynchronizeForkService : ISynchronizeForkService
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IGitClient gitClient;
        readonly IStatusBarNotificationService notifier;

        [ImportingConstructor]
        SynchronizeForkService(IGitClient gitClient, IStatusBarNotificationService notifier)
        {
            this.gitClient = gitClient;
            this.notifier = notifier;
        }

        public async Task<bool> Sync(ILocalRepositoryModel repo)
        {
            await ThreadingHelper.SwitchToPoolThreadAsync();

            var gitRepo = GitService.GitServiceHelper.GetRepository(repo.LocalPath);
            var upstream = gitRepo.Network.Remotes["upstream"];
            if (upstream == null)
                return false;

            await gitClient.Fetch(gitRepo, upstream.Name);
            var upstreamBranches = gitRepo.Branches.Where(x => x.IsRemote && x.Remote == upstream);
            foreach (var branch in gitRepo.Branches
                .Where(x => !x.IsRemote  &&
                            x.IsTracking &&
                            x.Remote.Name == "origin" &&
                            upstreamBranches.Any(r => r.FriendlyName == upstream.Name + "/" + x.FriendlyName) &&
                            !Equals(x, gitRepo.Head)
                            ))
            {
                var upstreamBranch = upstreamBranches.FirstOrDefault(x => x.FriendlyName == upstream.Name + "/" + branch.FriendlyName);
                var history = gitRepo.ObjectDatabase.CalculateHistoryDivergence(branch.Tip, upstreamBranch.Tip);
                if (history.AheadBy.HasValue && history.BehindBy.HasValue &&
                    history.AheadBy.Value == 0 && history.BehindBy.Value > 0)
                {
                    try
                    {
                        var forkBranch = gitRepo.Branches.First(x => x.IsRemote && x.FriendlyName == branch.Remote.Name + "/" + branch.FriendlyName);

                        // we can sync!
                        log.Debug("Fetching and merging " + history.BehindBy.Value + " commits to branch " + branch.FriendlyName + " from remote " + upstream.Name);
                        notifier.ShowMessage("Synchronizing " + branch.FriendlyName);
                        var refspec = string.Format(CultureInfo.InvariantCulture, "{0}:{0}", branch.FriendlyName);
                        var success = await gitClient.Fetch(gitRepo, upstream.Name, refspec).Catch(_ => false);
                        if (success)
                        {
                            history = gitRepo.ObjectDatabase.CalculateHistoryDivergence(upstreamBranch.Tip, forkBranch.Tip);
                            if (history.AheadBy.HasValue && history.BehindBy.HasValue &&
                                history.AheadBy.Value > 0 && history.BehindBy.Value == 0)
                            {
                                await gitClient.Push(gitRepo, branch)
                                    .Catch(ex => notifier.ShowError("Could not push ${branch.FriendlyName} to ${branch.Remote.Name}"));
                            }
                            else
                            {
                                notifier.ShowMessage(" ${branch.FriendlyName} already sync'd");
                            }
                        }
                        else
                        {
                            notifier.ShowError("Could not synchronize ${branch.FriendlyName}");
                        }
                    }
                    catch (Exception ee)
                    {
                        log.Debug("Sync failed", ee);
                        notifier.ShowError("Could not synchronize ${branch.FriendlyName}");
                    }
                }
            }
            notifier.ShowMessage("Synchronized");
            return true;
        }
    }
}
