using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akavache;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Services;
using Rothko;

namespace GitHub.App.Services
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Reactive;
    using System.Reactive.Linq;
    using Extensions.Reactive;
    using ReactiveUI;
    using Settings;
    using Guard = GitHub.Extensions.Guard;

    [Export(typeof(IUsageTracker))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UsageTracker : IUsageTracker
    {
        readonly IMetricsService client;

        readonly Lazy<ISharedCache> cache;
        readonly Lazy<IRepositoryHosts> repositoryHosts;
        //readonly Lazy<IAppVersionProvider> appVersionProvider;
        readonly Lazy<IEnvironment> environment;
        ////readonly Lazy<IRepositoryHosts> trackedRepositories;
        readonly IPackageSettings userSettings;

        // Whenever you add a counter make sure it gets added to _both_
        // BuildUsageModel and ClearCounters
        internal const string GHLastSubmissionKey = "GHLastSubmission";
        internal const string GHCommitCountKey = "GHCommitCount";
        internal const string GHSyncCountKey = "GHSyncCount";
        internal const string GHCloneCountKey = "GHCloneCount";
        internal const string GHShellLaunchCountKey = "GHShellLaunchCount";
        internal const string GHLaunchCountKeyDay = "GHLaunchCountDay";
        internal const string GHLaunchCountKeyWeek = "GHLaunchCountWeek";
        internal const string GHLaunchCountKeyMonth = "GHLaunchCountMonth";
        internal const string GHPartialCommitCount = "GHPartialCommitCount";
        internal const string GHTutorialRunCount = "GHTutorialRunCount";
        internal const string GHOpenInExplorerCount = "GHOpenInExplorerCount";
        internal const string GHOpenInShellCount = "GHOpenInShellCount";
        internal const string GHBranchSwitchCount = "GHBranchSwitchCount";
        internal const string GHDiscardChangesCount = "GHDiscardChangesCount";
        internal const string GHOpenedURLCount = "GHOpenedURLCount";
        internal const string GHLfsDiffCount = "GHLfsDiffCount";
        internal const string GHMergeCommitCount = "GHMergeCommitCount";
        internal const string GHMergeConflictCount = "GHMergeConflictCount";
        internal const string GHOpenInEditorCount = "GHOpenInEditorCount";
        internal const string GHUpstreamPullRequestCount = "GHUpstreamPullRequestCount";

        static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        IBlobCache localMachineCache { get { return cache.Value.LocalMachine; } }

        [ImportingConstructor]
        public UsageTracker(
            Lazy<ISharedCache> cache,
            ////Lazy<ITrackedRepositories> trackedRepositories,
            Lazy<IRepositoryHosts> repositoryHosts,
            ////Lazy<IAppVersionProvider> appVersionProvider,
            Lazy<IEnvironment> environment,
            IPackageSettings userSettings,
            IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(cache, "cache");
            ////Guard.ArgumentNotNull(trackedRepositories, "trackedRepositories");
            Guard.ArgumentNotNull(repositoryHosts, "repositoryHosts");
            ////Guard.ArgumentNotNull(appVersionProvider, "appVersionProvider");
            Guard.ArgumentNotNull(environment, "environment");
            Guard.ArgumentNotNull(userSettings, "userSettings");
            Guard.ArgumentNotNull(serviceProvider, "serviceProvider");

            this.cache = cache;
            ////this.trackedRepositories = trackedRepositories;
            this.repositoryHosts = repositoryHosts;
            ////this.appVersionProvider = appVersionProvider;
            this.environment = environment;
            this.userSettings = userSettings;
            this.client = (IMetricsService)serviceProvider.GetService(typeof(IMetricsService));
        }

        IObservable<Unit> SubmitIfNeeded()
        {
            if (client != null)
            {
                return GetLastUpdated()
                    .SelectMany(lastSubmission =>
                    {
                        var now = RxApp.MainThreadScheduler.Now;

                        var lastDate = lastSubmission.LocalDateTime.Date;
                        var currentDate = now.LocalDateTime.Date;

                        // New day, new stats. This matches the GHfM implementation
                        // of when to send stats.
                        if (lastDate == currentDate)
                        {
                            return Observable.Return(Unit.Default);
                        }

                        // Every time we increment the launch count we increment both GHLaunchCountKeyDay
                        // and GHLaunchCountKeyWeek but we only submit (and clear) the GHLaunchCountKeyWeek
                        // when we've transitioned into a new week. We've defined a week by the ISO8601 
                        // definition, i.e. week starting on Monday and ending on Sunday.
                        var includeWeekly = GetIso8601WeekOfYear(lastDate) != GetIso8601WeekOfYear(currentDate);
                        var includeMonthly = lastDate.Month != currentDate.Month;

                        return BuildUsageModel(includeWeekly, includeMonthly)
                            .SelectMany(client.PostUsage)
                            .Concat(ClearCounters(includeWeekly, includeMonthly))
                            .Concat(StoreLastUpdated(now));
                    })
                    .AsCompletion();
            }
            else
            {
                return Observable.Return(Unit.Default);
            }
        }

        IObservable<DateTimeOffset> GetLastUpdated()
        {
            return Observable.Defer(() => localMachineCache.GetObject<DateTimeOffset>(GHLastSubmissionKey))
                .Catch<DateTimeOffset, KeyNotFoundException>(_ => Observable.Return(DateTimeOffset.MinValue));
        }

        IObservable<Unit> StoreLastUpdated(DateTimeOffset lastUpdated)
        {
            return Observable.Defer(() => localMachineCache.InsertObject(GHLastSubmissionKey, lastUpdated));
        }

        static Calendar cal = CultureInfo.InvariantCulture.Calendar;

        // http://blogs.msdn.com/b/shawnste/archive/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net.aspx
        static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = cal.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return cal.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        IObservable<Unit> ClearCounters(bool weekly, bool monthly)
        {
            var standardCounters = new[] {
                GHCommitCountKey,
                GHSyncCountKey,
                GHCloneCountKey,
                GHShellLaunchCountKey,
                GHLaunchCountKeyDay,
                GHPartialCommitCount,
                GHTutorialRunCount,
                GHOpenInExplorerCount,
                GHOpenInShellCount,
                GHBranchSwitchCount,
                GHDiscardChangesCount,
                GHOpenedURLCount,
                GHLfsDiffCount,
                GHMergeCommitCount,
                GHMergeConflictCount,
                GHOpenInEditorCount,
                GHUpstreamPullRequestCount
            };

            var counters = standardCounters
                .Concat(weekly ? new[] { GHLaunchCountKeyWeek } : Enumerable.Empty<string>())
                .Concat(monthly ? new[] { GHLaunchCountKeyMonth } : Enumerable.Empty<string>());

            return counters
                .Select(ClearCounter)
                .Merge()
                .AsCompletion();
        }

        IObservable<Unit> ClearCounter(string key)
        {
            return Observable.Defer(() => localMachineCache.InvalidateObject<int>(key));
        }

        IObservable<int> GetCounter(string key)
        {
            return Observable.Defer(() => localMachineCache.GetObject<int>(key))
                .Catch<int, KeyNotFoundException>(_ => Observable.Return(0));
        }

        IObservable<Unit> SaveCounter(string key, int value)
        {
            return Observable.Defer(() => localMachineCache.InsertObject(key, value));
        }

        IObservable<UsageModel> BuildUsageModel(bool weekly, bool monthly)
        {
            ////var repositories = trackedRepositories.Value.Repositories;
            var hosts = repositoryHosts.Value;

            var model = new UsageModel
            {
                ////NumberOfRepositories = repositories.Count,
                ////NumberOfGitHubRepositories = repositories.Count(x => x.IsHosted),
                ////NumberOfGitHubForks = repositories.Count(x => x.IsHosted && x.IsFork),
                ////NumberOfRepositoryOwners = repositories.Count(r => IsOwner(r, hosts))
            };


            if (hosts.GitHubHost?.IsLoggedIn == true)
            {
                model.IsGitHubUser = true;
                ////model.NumberOfOrgs = gitHubHost.Organizations.Count();
            }

            if (hosts.EnterpriseHost?.IsLoggedIn == true)
            {
                model.IsEnterpriseUser = true;
            }

            var env = environment.Value;

            model.OsVersion = env.OSVersion.Version.ToString();
            model.Is64BitOperatingSystem = env.Is64BitOperatingSystem;
            model.Lang = CultureInfo.InstalledUICulture.IetfLanguageTag;

            ////try
            ////{
            ////    model.RamMB = (int)(env.GetTotalInstalledPhysicalMemory() / 1024 / 1024);
            ////}
            ////catch (Exception ex)
            ////{
            ////    // This shouldn't really throw but let's be super defensive.
            ////    log.Warn("Could not get total installed physical memory", ex);
            ////}

            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var elapsedSinceStart = DateTime.Now - currentProcess.StartTime;
                model.SecondsSinceLaunch = Math.Max(0, (int)elapsedSinceStart.TotalSeconds);
            }
            catch (Exception ex)
            {
                log.Warn("Could not get process uptime", ex);
            }

            ////model.AppVersion = appVersionProvider.Value.Version.ToString();

            var counters = new List<IObservable<int>>
            {
                GetCounter(GHCommitCountKey).Do(x => model.NumberOfCommits = x),
                GetCounter(GHCloneCountKey).Do(x => model.NumberOfClones = x),
                GetCounter(GHSyncCountKey).Do(x => model.NumberOfSyncs = x),

                // NB: We're using this in a slightly different way than MAC. On mac this means
                // whether or not a user has installed their command line tools, for us (since it's
                // always installed I've made it track whether or not the user has launched the Git 
                // shell. We might be able to get some insight into how many of our users use the CLI.
                GetCounter(GHShellLaunchCountKey).Do(x => model.InstalledCommandLineTools = x > 0),
                GetCounter(GHLaunchCountKeyDay).Do(x => model.NumberOfStartups = x),
                GetCounter(GHPartialCommitCount).Do(x => model.NumberOfPartialCommits = x),
                GetCounter(GHTutorialRunCount).Do(x => model.NumberOfTutorialRuns = x),
                GetCounter(GHOpenInExplorerCount).Do(x => model.NumberOfOpenOnDisks = x),
                GetCounter(GHOpenInShellCount).Do(x => model.NumberOfOpenInShells = x),
                GetCounter(GHBranchSwitchCount).Do(x => model.NumberOfBranchSwitches = x),
                GetCounter(GHDiscardChangesCount).Do(x => model.NumberOfDiscardChanges = x),
                GetCounter(GHOpenedURLCount).Do(x => model.NumberOfOpenedURLs = x),
                GetCounter(GHLfsDiffCount).Do(x => model.NumberOfLFSDiffs = x),
                GetCounter(GHMergeCommitCount).Do(x => model.NumberOfMergeCommits = x),
                GetCounter(GHMergeConflictCount).Do(x => model.NumberOfMergeConflicts = x),
                GetCounter(GHOpenInEditorCount).Do(x => model.NumberOfOpenInExternalEditors = x),
                GetCounter(GHUpstreamPullRequestCount).Do(x => model.NumberOfUpstreamPullRequests = x),
            };

            if (weekly)
            {
                counters.Add(GetCounter(GHLaunchCountKeyWeek)
                    .Do(x => model.NumberOfStartupsWeek = x));
            }

            if (monthly)
            {
                counters.Add(GetCounter(GHLaunchCountKeyMonth)
                    .Do(x => model.NumberOfStartupsMonth = x));
            }

            return Observable.Merge(counters)
                .ContinueAfter(() => Observable.Return(model));
        }

        static bool IsOwner(IRepositoryModel repo, IRepositoryHosts hosts)
        {
            Guard.ArgumentNotNull(repo, "repo");
            Guard.ArgumentNotNull(hosts, "hosts");

            ////if (!repo.IsHosted || !repo.OwnerId.HasValue)
            ////    return false;

            ////if (hosts.GitHubHost != null && IsOwner(repo, hosts.GitHubHost))
            ////    return true;

            ////if (hosts.EnterpriseHost != null && IsOwner(repo, hosts.EnterpriseHost))
            ////    return true;

            return false;
        }

        ////static bool IsOwner(IRepositoryModel repo, IRepositoryHost host)
        ////{
        ////    Guard.ArgumentNotNull(repo, "repo");
        ////    Guard.ArgumentNotNull(host, "host");

        ////    return host.User != null && repo.OwnerId.HasValue && host.User.Id == repo.OwnerId.Value;
        ////}

        IObservable<Unit> Run()
        {
            return Observable.Defer(() =>
            {
                // We have a lot of stuff going on at startup so let's wait a little while before we submit our usage stats
                return Observable.Timer(TimeSpan.FromMinutes(1), TimeSpan.FromHours(8), RxApp.TaskpoolScheduler)
                    .SelectMany(_ =>
                        SubmitIfNeeded()
                            .Catch<Unit, Exception>(ex =>
                            {
                                log.Warn("Failed submitting usage data", ex);
                                return Observable.Return(Unit.Default);
                            }));
            });
        }

        IObservable<int> IncrementCounter(string key)
        {
            return Observable.Defer(() =>
            {
                try
                {
                    // Don't even store data locally if the user opts out.
                    if (!userSettings.CollectMetrics)
                    {
                        return Observable.Empty<int>();
                    }

                    return GetCounter(key)
                        .Select(x => x + 1)
                        .SelectMany(x => SaveCounter(key, x).Select(_ => x))
                        .Catch<int, Exception>(ex =>
                        {
                            log.Warn("Could not increment usage data counter", ex);
                            return Observable.Empty<int>();
                        });
                }
                catch (Exception ex)
                {
                    // This should never happen, we have a catch in the observable
                    // chain and se subscribe on the task pool but just to be extra
                    // cautious let's deal with this scenario as well because this
                    // method should never throw.
                    log.Warn("Failed incrementing usage data counter", ex);
                    return Observable.Empty<int>();
                }
            });
        }

        public void IncrementCommitCount()
        {
            IncrementCounter(GHCommitCountKey)
                .Subscribe();
        }

        public void IncrementSyncCount()
        {
            IncrementCounter(GHSyncCountKey)
                .Subscribe();
        }

        public void IncrementCloneCount()
        {
            IncrementCounter(GHCloneCountKey)
                .Subscribe();
        }

        public void IncrementShellLaunchCount()
        {
            IncrementCounter(GHShellLaunchCountKey)
                .Subscribe();
        }

        public void IncrementLfsDiffCount()
        {
            IncrementCounter(GHLfsDiffCount)
                .Subscribe();
        }

        public void IncrementLaunchCount()
        {
            IncrementCounter(GHLaunchCountKeyDay)
                .ContinueAfter(() => IncrementCounter(GHLaunchCountKeyWeek))
                .ContinueAfter(() => IncrementCounter(GHLaunchCountKeyMonth))
                .Subscribe();
        }

        public void IncrementPartialCommitCount()
        {
            IncrementCounter(GHPartialCommitCount)
                .Subscribe();
        }

        public void IncrementTutorialRunCount()
        {
            IncrementCounter(GHTutorialRunCount)
                .Subscribe();
        }

        public void IncrementOpenInExplorerCount()
        {
            IncrementCounter(GHOpenInExplorerCount)
                .Subscribe();
        }
        public void IncrementOpenInShellCount()
        {
            IncrementCounter(GHOpenInShellCount)
                .Subscribe();
        }

        public void IncrementBranchSwitchCount()
        {
            IncrementCounter(GHBranchSwitchCount)
                .Subscribe();
        }

        public void IncrementDiscardChangesCount()
        {
            IncrementCounter(GHDiscardChangesCount)
                .Subscribe();
        }

        public void IncrementNumberOfOpenedURLs()
        {
            IncrementCounter(GHOpenedURLCount)
                .Subscribe();
        }

        public void IncrementMergeCommitCount()
        {
            IncrementCounter(GHMergeCommitCount)
                .Subscribe();
        }

        public void IncrementMergeConflictCount()
        {
            IncrementCounter(GHMergeConflictCount)
                .Subscribe();
        }

        public void IncrementOpenInEditorCount()
        {
            IncrementCounter(GHOpenInEditorCount)
                .Subscribe();
        }

        public void IncrementUpstreamPullRequestCount()
        {
            IncrementCounter(GHUpstreamPullRequestCount)
                .Subscribe();
        }

        IObservable<Unit> OptOut()
        {
            if (client != null)
            {
                return Observable.Defer(() =>
                {
                    log.Info("User has disabled sending anonymized usage statistics to GitHub");

                    return ClearCounters(true, true)
                        .ContinueAfter(() => client.SendOptOut());
                });
            }
            else
            {
                return Observable.Return(Unit.Default);
            }
        }

        IObservable<Unit> OptIn()
        {
            if (client != null)
            {
                return Observable.Defer(() =>
                {
                    log.Info("User has enabled sending anonymized usage statistics to GitHub");

                    return client.SendOptIn();
                });
            }
            else
            {
                return Observable.Return(Unit.Default);
            }
        }
    }
}
