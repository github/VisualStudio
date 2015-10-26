using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.ViewModels
{
    [Export(typeof(ICloneSyncViewModel))]
    public class CloneSyncViewModel : ICloneSyncViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Lazy<IGitClient> gitClient;

        [ImportingConstructor]
        CloneSyncViewModel(Lazy<IGitClient> gitClient)
        {
            this.gitClient = gitClient;
        }

        public IObservable<bool> Sync(ISimpleRepositoryModel repo)
        {
            var gitRepo = GitService.GetRepoFromPath(repo.LocalPath);
            var upstream = gitRepo.Network.Remotes["upstream"];
            if (upstream == null)
                return Observable.Return(false);

            var upstreamBranches = gitRepo.Branches.Where(x => x.IsRemote && x.Remote == upstream);
            foreach (var branch in gitRepo.Branches
                .Where(x => !x.IsRemote  &&
                            x.IsTracking &&
                            x.Remote.Name == "origin" &&
                            upstreamBranches.Any(r => r.Name == upstream.Name + "/" + x.Name) &&
                            !Equals(x, gitRepo.Head)
                            ))
            {
                var upstreamBranch = upstreamBranches.FirstOrDefault(x => x.Name == upstream.Name + "/" + branch.Name);
                var history = gitRepo.ObjectDatabase.CalculateHistoryDivergence(branch.Tip, upstreamBranch.Tip);
                if (history.AheadBy.HasValue && history.BehindBy.HasValue &&
                    history.AheadBy.Value == 0 && history.BehindBy.Value > 0)
                {
                    // we can sync!
                    log.Debug("Fetching and merging " + history.BehindBy.Value + " commits to branch " + branch.Name + " from remote " + upstream.Name);
                    var refspec = string.Format(CultureInfo.InvariantCulture, "{0}:{0}", branch.Name);
                    return gitClient.Value.Fetch(gitRepo, upstream.Name, refspec)
                        .Select(x => gitClient.Value.Push(gitRepo, branch.Name, branch.Remote.Name))
                        .Select(x => true);
                }
            }


            return Observable.Return(true);
        }
    }
}
