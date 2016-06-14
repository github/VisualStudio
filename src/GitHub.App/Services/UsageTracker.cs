using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Caches;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Settings;
using ReactiveUI;
using Rothko;

#pragma warning disable CS0649

namespace GitHub.Services
{
    [Export(typeof(IUsageTracker))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UsageTracker : IUsageTracker
    {
        const string GHLastSubmissionKey = "GHLastSubmission";
        const string GHCreateCountKey = "GHCreateCountKey";
        const string GHCloneCountKey = "GHCloneCount";
        const string GHPublishCountKey = "GHPublishCountKey";
        const string GHCreateGistCountKey = "GHCreateGistCountKey";
        const string GHOpenInGitHubCountKey = "GHOpenInGitHubCountKey";
        const string GHLinkToGitHubCountKey = "GHLinkToGitHubCountKey";
        const string GHUpstreamPullRequestCount = "GHUpstreamPullRequestCount";
        const string GHLoginCountKey = "GHLoginCountKey";
        const string GHLaunchCountKeyDay = "GHLaunchCountDay";
        const string GHLaunchCountKeyWeek = "GHLaunchCountWeek";
        const string GHLaunchCountKeyMonth = "GHLaunchCountMonth";

        readonly Dictionary<string, Action<UsageModel, int>> standardCounters = new Dictionary<string, Action<UsageModel, int>> {
                {  GHCloneCountKey, (model, x) => model.NumberOfClones = x },
                {  GHCreateCountKey, (model, x) => model.NumberOfReposCreated = x },
                {  GHPublishCountKey, (model, x) => model.NumberOfReposPublished = x },
                {  GHCreateGistCountKey, (model, x) => model.NumberOfGists = x },
                {  GHOpenInGitHubCountKey, (model, x) => model.NumberOfOpenInGitHub = x },
                {  GHLinkToGitHubCountKey, (model, x) => model.NumberOfLinkToGitHub = x },
                {  GHUpstreamPullRequestCount, (model, x) => model.NumberOfUpstreamPullRequests = x },
                {  GHLoginCountKey, (model, x) => model.NumberOfLogins = x },
                {  GHLaunchCountKeyDay, (model, x) => model.NumberOfStartups = x },
                {  GHLaunchCountKeyWeek, (model, x) => model.NumberOfStartupsWeek = x },
                {  GHLaunchCountKeyMonth, (model, x) => model.NumberOfStartupsMonth = x },
        };

        static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        readonly IMetricsService client;
        readonly Lazy<ISharedCache> cache;
        readonly Lazy<IRepositoryHosts> repositoryHosts;
        readonly IPackageSettings userSettings;

        [ImportingConstructor]
        public UsageTracker(
            Lazy<ISharedCache> cache,
            Lazy<IRepositoryHosts> repositoryHosts,
            IPackageSettings userSettings,
            IUIProvider serviceProvider)
        {
            this.cache = cache;
            this.repositoryHosts = repositoryHosts;
            this.userSettings = userSettings;
            this.client = (IMetricsService)serviceProvider.GetService(typeof(IMetricsService));

            userSettings.WhenAny(x => x.CollectMetrics, x => x.Value)
                // If SendUsageData is true we'll subscribe to the Run()
                // observable (which is a timer) and thus kick off the
                // tracker. The Run method will produce on unit per tick
                // but we don't care about that at all so we'll ignore
                // that and instead send true or false down the chain
                // indicating that the tracker is running or not.
                .Select(sendUsageData => sendUsageData
                    ? Run()
                        .IgnoreElements()
                        .Select(_ => true)
                        .StartWith(true)
                    : Observable.Return(false))
                .Switch()
                // The first value will be what initially kicks off the
                // tracker. The second value (if there is one) will signal
                // that the user has opted in or out of usage tracking.
                .Skip(1)
                .Select(sendUsageData => sendUsageData ? OptIn() : OptOut())
                .Switch()
                .Subscribe();
        }

        IBlobCache LocalMachineCache => cache.Value.LocalMachine;

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
                            return Observable.Return(Unit.Default);

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
            return Observable.Defer(() => LocalMachineCache.GetObject<DateTimeOffset>(GHLastSubmissionKey))
                .Catch<DateTimeOffset, KeyNotFoundException>(_ => Observable.Return(DateTimeOffset.MinValue));
        }

        IObservable<Unit> StoreLastUpdated(DateTimeOffset lastUpdated)
        {
            return Observable.Defer(() => LocalMachineCache.InsertObject(GHLastSubmissionKey, lastUpdated));
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
            var counters = standardCounters.Keys
                .Except(new [] { GHLastSubmissionKey })
                .Except(weekly ? new[] { GHLaunchCountKeyWeek } : Enumerable.Empty<string>())
                .Except(monthly ? new[] { GHLaunchCountKeyMonth } : Enumerable.Empty<string>());

            return counters
                .Select(ClearCounter)
                .Merge()
                .AsCompletion();
        }

        IObservable<Unit> ClearCounter(string key)
        {
            return Observable.Defer(() => LocalMachineCache.InvalidateObject<int>(key));
        }

        IObservable<int> GetCounter(string key)
        {
            return Observable.Defer(() => LocalMachineCache.GetObject<int>(key))
                .Catch<int, KeyNotFoundException>(_ => Observable.Return(0));
        }

        IObservable<Unit> SaveCounter(string key, int value)
        {
            return Observable.Defer(() => LocalMachineCache.InsertObject(key, value));
        }

        IObservable<UsageModel> BuildUsageModel(bool weekly, bool monthly)
        {
            var hosts = repositoryHosts.Value;

            var model = new UsageModel();

            if (hosts.GitHubHost?.IsLoggedIn == true)
                model.IsGitHubUser = true;

            if (hosts.EnterpriseHost?.IsLoggedIn == true)
                model.IsEnterpriseUser = true;

            model.Lang = CultureInfo.InstalledUICulture.IetfLanguageTag;
            model.AppVersion = AssemblyVersionInformation.Version;
            model.VSVersion = VisualStudio.Services.VisualStudioVersion;

            var counters = standardCounters
                .Where(x => x.Key != GHLastSubmissionKey &&               // filter out last submission date
                            (!weekly && x.Key == GHLaunchCountKeyWeek) && // filter out weekly metrics if !weekly
                            (!monthly && x.Key == GHLaunchCountKeyMonth)  // filter out monthly metrics if !monthly
                      )
                .ToObservable()
                .SelectMany(c => GetCounter(GHLaunchCountKeyDay).Do(x => c.Value(model, x))); // set metric in model

            return Observable.Merge(counters)
                .ContinueAfter(() => Observable.Return(model));
        }

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
                        .Do(x => Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Incremented {0} to {1}", key, x)))
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

        public void IncrementCloneCount()
        {
            IncrementCounter(GHCloneCountKey)
                .Subscribe();
        }

        public void IncrementCreateCount()
        {
            IncrementCounter(GHCreateCountKey)
                .Subscribe();
        }

        public void IncrementLaunchCount()
        {
            IncrementCounter(GHLaunchCountKeyDay)
                .ContinueAfter(() => IncrementCounter(GHLaunchCountKeyWeek))
                .ContinueAfter(() => IncrementCounter(GHLaunchCountKeyMonth))
                .Subscribe();
        }

        public void IncrementPublishCount()
        {
            IncrementCounter(GHPublishCountKey)
                .Subscribe();
        }

        public void IncrementOpenInGitHubCount()
        {
            IncrementCounter(GHOpenInGitHubCountKey)
                .Subscribe();
        }

        public void IncrementLinkToGitHubCount()
        {
            IncrementCounter(GHLinkToGitHubCountKey)
                .Subscribe();
        }

        public void IncrementCreateGistCount()
        {
            IncrementCounter(GHCreateGistCountKey)
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
