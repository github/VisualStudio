using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using GitHub.Models;
using GitHub.Settings;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    [Export(typeof(IUsageTracker))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UsageTracker : IUsageTracker
    {
        const string StoreFileName = "ghfvs.usage";
        //static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        static readonly Calendar cal = CultureInfo.InvariantCulture.Calendar;

        readonly IMetricsService client;
        readonly Lazy<IConnectionManager> connectionManager;
        readonly IPackageSettings userSettings;
        readonly DispatcherTimer timer;
        readonly string storePath;
        UsageStore usage;

        [ImportingConstructor]
        public UsageTracker(
            IProgram program,
            Lazy<IConnectionManager> connectionManager,
            IPackageSettings userSettings,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            var componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;

            this.connectionManager = connectionManager;
            this.userSettings = userSettings;
            this.client = componentModel?.DefaultExportProvider.GetExportedValue<IMetricsService>();
            this.timer = new DispatcherTimer(
                TimeSpan.FromMinutes(1),
                DispatcherPriority.Background,
                TimerTick,
                Dispatcher.CurrentDispatcher);
            this.storePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                program.ApplicationName,
                StoreFileName);
            this.usage = LoadUsage();

            userSettings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(userSettings.CollectMetrics))
                {
                    UpdateTimer(false);
                }
            };

            UpdateTimer(true);
        }

        public void IncrementLaunchCount()
        {
            ++usage.Model.NumberOfStartups;
            ++usage.Model.NumberOfStartupsWeek;
            ++usage.Model.NumberOfStartupsMonth;
            SaveUsage(usage);
        }

        public void IncrementCloneCount()
        {
            ++usage.Model.NumberOfClones;
            SaveUsage(usage);
        }

        public void IncrementCreateCount()
        {
            ++usage.Model.NumberOfReposCreated;
            SaveUsage(usage);
        }

        public void IncrementPublishCount()
        {
            ++usage.Model.NumberOfReposPublished;
            SaveUsage(usage);
        }

        public void IncrementOpenInGitHubCount()
        {
            ++usage.Model.NumberOfOpenInGitHub;
            SaveUsage(usage);
        }

        public void IncrementLinkToGitHubCount()
        {
            ++usage.Model.NumberOfLinkToGitHub;
            SaveUsage(usage);
        }

        public void IncrementCreateGistCount()
        {
            ++usage.Model.NumberOfGists;
            SaveUsage(usage);
        }

        public void IncrementUpstreamPullRequestCount()
        {
            ++usage.Model.NumberOfClones;
            SaveUsage(usage);
        }

        UsageStore LoadUsage()
        {
            var result = File.Exists(storePath) ?
                SimpleJson.DeserializeObject<UsageStore>(File.ReadAllText(storePath)) :
                new UsageStore { Model = new UsageModel() };

            result.Model.Lang = CultureInfo.InstalledUICulture.IetfLanguageTag;
            result.Model.AppVersion = AssemblyVersionInformation.Version;
            result.Model.VSVersion = GitHub.VisualStudio.Services.VisualStudioVersion;

            return result;
        }

        void SaveUsage(UsageStore store)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(storePath));
            var json = SimpleJson.SerializeObject(store);
            File.WriteAllText(storePath, json);
        }

        void UpdateTimer(bool initialCall)
        {
            // If the method was called due to userSettings.CollectMetrics changing, then send an
            // opt-in/out message.
            if (!initialCall && client != null)
            {
                if (userSettings.CollectMetrics)
                    client.SendOptIn();
                else
                    client.SendOptOut();
            }

            // The timer first ticks after 1 minute to allow things to settle down after startup.
            // This will be changed to 8 hours after the first tick by the TimerTick method.
            timer.Stop();
            timer.Interval = TimeSpan.FromMinutes(1);

            if (userSettings.CollectMetrics && client != null)
                timer.Start();
        }

        async void TimerTick(object sender, EventArgs e)
        {
            Debug.Assert(client != null, "TimerTick should not be triggered when there is no IMetricsService");

            // Subsequent timer ticks should occur every 8 hours.
            timer.Interval = TimeSpan.FromHours(8);

            try
            {
                // Every time we increment the launch count we increment both daily and weekly 
                // launch count but we only submit (and clear) the weekly launch count when we've
                // transitioned into a new week. We've defined a week by the ISO8601 definition,
                // i.e. week starting on Monday and ending on Sunday.
                var lastDate = usage.LastUpdated;
                var currentDate = DateTimeOffset.Now;
                var includeWeekly = GetIso8601WeekOfYear(lastDate) != GetIso8601WeekOfYear(currentDate);
                var includeMonthly = lastDate.Month != currentDate.Month;

                // Only send stats once a day.
                if (lastDate != currentDate)
                {
                    await SendUsage(includeWeekly, includeMonthly);
                }

                ClearCounters(includeWeekly, includeMonthly);
                usage.LastUpdated = DateTimeOffset.Now.UtcDateTime;
                SaveUsage(usage);
            }
            catch //(Exception ex)
            {
                //log.Warn("Failed submitting usage data", ex);
            }
        }

        async Task SendUsage(bool weekly, bool monthly)
        {
            Debug.Assert(client != null, "SendUsage should not be called when there is no IMetricsService");

            var connectionManager = this.connectionManager.Value;

            if (connectionManager.Connections.Any(x => x.HostAddress.IsGitHubDotCom()))
            {
                usage.Model.IsGitHubUser = true;
            }

            if (connectionManager.Connections.Any(x => !x.HostAddress.IsGitHubDotCom()))
            {
                usage.Model.IsEnterpriseUser = true;
            }

            var model = usage.Model.Clone(weekly, monthly);
            await client.PostUsage(model);
        }

        // http://blogs.msdn.com/b/shawnste/archive/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net.aspx
        static int GetIso8601WeekOfYear(DateTimeOffset time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = cal.GetDayOfWeek(time.UtcDateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return cal.GetWeekOfYear(time.UtcDateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        void ClearCounters(bool weekly, bool monthly)
        {
            usage.Model.NumberOfStartups = 0;
            usage.Model.NumberOfClones = 0;
            usage.Model.NumberOfReposCreated = 0;
            usage.Model.NumberOfReposPublished = 0;
            usage.Model.NumberOfGists = 0;
            usage.Model.NumberOfOpenInGitHub = 0;
            usage.Model.NumberOfLinkToGitHub = 0;
            usage.Model.NumberOfLogins = 0;
            usage.Model.NumberOfUpstreamPullRequests = 0;

            if (weekly)
                usage.Model.NumberOfStartupsWeek = 0;

            if (monthly)
                usage.Model.NumberOfStartupsMonth = 0;
        }

        class UsageStore
        {
            public DateTimeOffset LastUpdated { get; set; }
            public UsageModel Model { get; set; }
        }
    }
}
