using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.Settings;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    public sealed class UsageTracker : IUsageTracker, IDisposable
    {
        readonly IGitHubServiceProvider gitHubServiceProvider;
        bool initialized;
        IMetricsService client;
        IUsageService service;
        IConnectionManager connectionManager;
        IPackageSettings userSettings;
        IVSServices vsservices;
        IDisposable timer;
        bool firstTick = true;

        [ImportingConstructor]
        public UsageTracker(
            IGitHubServiceProvider gitHubServiceProvider,
            IUsageService service)
        {
            this.gitHubServiceProvider = gitHubServiceProvider;
            this.service = service;
            timer = StartTimer();            
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public async Task IncrementLaunchCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfStartups;
            ++usage.Model.NumberOfStartupsWeek;
            ++usage.Model.NumberOfStartupsMonth;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementCloneCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfClones;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementCreateCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfReposCreated;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPublishCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfReposPublished;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementOpenInGitHubCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfOpenInGitHub;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementLinkToGitHubCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfLinkToGitHub;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementCreateGistCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfGists;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementUpstreamPullRequestCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfUpstreamPullRequests;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementLoginCount()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfLogins;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPullRequestCheckOutCount(bool fork)
        {
            var usage = await LoadUsage();

            if (fork)
                ++usage.Model.NumberOfForkPullRequestsCheckedOut;
            else
                ++usage.Model.NumberOfLocalPullRequestsCheckedOut;

            await service.WriteLocalData(usage);
        }

        public async Task IncrementPullRequestPushCount(bool fork)
        {
            var usage = await LoadUsage();

            if (fork)
                ++usage.Model.NumberOfForkPullRequestPushes;
            else
                ++usage.Model.NumberOfLocalPullRequestPushes;

            await service.WriteLocalData(usage);
        }

        public async Task IncrementPullRequestPullCount(bool fork)
        {
            var usage = await LoadUsage();

            if (fork)
                ++usage.Model.NumberOfForkPullRequestPulls;
            else
                ++usage.Model.NumberOfLocalPullRequestPulls;

            await service.WriteLocalData(usage);
        }

        public async Task IncrementWelcomeDocsClicks()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfWelcomeDocsClicks;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementWelcomeTrainingClicks()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfWelcomeTrainingClicks;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementGitHubPaneHelpClicks()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfGitHubPaneHelpClicks;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPullRequestOpened()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfPullRequestsOpened;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPRDetailsViewChanges()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfPRDetailsViewChanges;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPRDetailsViewFile()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfPRDetailsViewFile;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPRDetailsCompareWithSolution()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfPRDetailsCompareWithSolution;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPRDetailsOpenFileInSolution()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfPRDetailsOpenFileInSolution;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPRReviewDiffViewInlineCommentOpen()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfPRReviewDiffViewInlineCommentOpen;
            await service.WriteLocalData(usage);
        }

        public async Task IncrementPRReviewDiffViewInlineCommentPost()
        {
            var usage = await LoadUsage();
            ++usage.Model.NumberOfPRReviewDiffViewInlineCommentPost;
            await service.WriteLocalData(usage);
        }

        IDisposable StartTimer()
        {
            return service.StartTimer(TimerTick, TimeSpan.FromMinutes(3), TimeSpan.FromHours(8));
        }

        async Task Initialize()
        {
            // The services needed by the usage tracker are loaded when they are first needed to
            // improve the startup time of the extension.
            if (!initialized)
            {
                await ThreadingHelper.SwitchToMainThreadAsync();

                client = gitHubServiceProvider.TryGetService<IMetricsService>();
                connectionManager = gitHubServiceProvider.GetService<IConnectionManager>();
                userSettings = gitHubServiceProvider.GetService<IPackageSettings>();
                vsservices = gitHubServiceProvider.GetService<IVSServices>();
                initialized = true;
            }
        }

        async Task<UsageData> LoadUsage()
        {
            await Initialize();

            var result = await service.ReadLocalData();
            result.Model.Lang = CultureInfo.InstalledUICulture.IetfLanguageTag;
            result.Model.AppVersion = AssemblyVersionInformation.Version;
            result.Model.VSVersion = vsservices.VSVersion;
            return result;
        }

        async Task TimerTick()
        {
            await Initialize();

            if (firstTick)
            {
                await IncrementLaunchCount();
                firstTick = false;
            }

            if (client == null || !userSettings.CollectMetrics)
            {
                timer.Dispose();
                timer = null;
                return;
            }

            // Every time we increment the launch count we increment both daily and weekly
            // launch count but we only submit (and clear) the weekly launch count when we've
            // transitioned into a new week. We've defined a week by the ISO8601 definition,
            // i.e. week starting on Monday and ending on Sunday.
            var usage = await LoadUsage();
            var lastDate = usage.LastUpdated;
            var currentDate = DateTimeOffset.Now;
            var includeWeekly = !service.IsSameWeek(usage.LastUpdated);
            var includeMonthly = !service.IsSameMonth(usage.LastUpdated);

            // Only send stats once a day.
            if (!service.IsSameDay(usage.LastUpdated))
            {
                await SendUsage(usage.Model, includeWeekly, includeMonthly);
                ClearCounters(usage.Model, includeWeekly, includeMonthly);
                usage.LastUpdated = DateTimeOffset.Now.UtcDateTime;
                await service.WriteLocalData(usage);
            }
        }

        async Task SendUsage(UsageModel usage, bool weekly, bool monthly)
        {
            if (client == null)
            {
                throw new GitHubLogicException("SendUsage should not be called when there is no IMetricsService");
            }

            if (connectionManager.Connections.Any(x => x.HostAddress.IsGitHubDotCom()))
            {
                usage.IsGitHubUser = true;
            }

            if (connectionManager.Connections.Any(x => !x.HostAddress.IsGitHubDotCom()))
            {
                usage.IsEnterpriseUser = true;
            }

            var model = usage.Clone(weekly, monthly);
            await client.PostUsage(model);
        }

        static void ClearCounters(UsageModel usage, bool weekly, bool monthly)
        {
            var properties = usage.GetType().GetRuntimeProperties();

            foreach (var property in properties)
            {
                var setValue = property.PropertyType == typeof(int);

                if (property.Name == nameof(usage.NumberOfStartupsWeek))
                    setValue = weekly;
                else if (property.Name == nameof(usage.NumberOfStartupsMonth))
                    setValue = monthly;

                if (setValue)
                {
                    property.SetValue(usage, 0);
                }
            }
        }
    }
}
