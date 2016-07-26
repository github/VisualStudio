using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using GitHub.Models;
using GitHub.Settings;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using GitHub.Extensions;

namespace GitHub.Services
{
    [Export(typeof(IUsageTracker))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UsageTracker : IUsageTracker
    {
        const string StoreFileName = "ghfvs.usage";
        static readonly Calendar cal = CultureInfo.InvariantCulture.Calendar;

        readonly IMetricsService client;
        readonly IConnectionManager connectionManager;
        readonly IPackageSettings userSettings;
        readonly DispatcherTimer timer;
        readonly string storePath;

        Func<string, bool> fileExists;
        Func<string, Encoding, string> readAllText;
        Action<string, string, Encoding> writeAllText;
        Action<string> dirCreate;

        [ImportingConstructor]
        public UsageTracker(
            IProgram program,
            IConnectionManager connectionManager,
            IPackageSettings userSettings,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            fileExists = (path) => System.IO.File.Exists(path);
            readAllText = (path, encoding) => System.IO.File.ReadAllText(path, encoding);
            writeAllText = (path, content, encoding) => System.IO.File.WriteAllText(path, content, encoding);
            dirCreate = (path) => System.IO.Directory.CreateDirectory(path);

            this.connectionManager = connectionManager;
            this.userSettings = userSettings;
            this.client = serviceProvider.GetExportedValue<IMetricsService>();
            this.timer = new DispatcherTimer(
                TimeSpan.FromMinutes(1),
                DispatcherPriority.Background,
                TimerTick,
                Dispatcher.CurrentDispatcher);
            this.storePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                program.ApplicationName,
                StoreFileName);

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
            var usage = LoadUsage();
            ++usage.Model.NumberOfStartups;
            ++usage.Model.NumberOfStartupsWeek;
            ++usage.Model.NumberOfStartupsMonth;
            SaveUsage(usage);
        }

        public void IncrementCloneCount()
        {
            var usage = LoadUsage();
            ++usage.Model.NumberOfClones;
            SaveUsage(usage);
        }

        public void IncrementCreateCount()
        {
            var usage = LoadUsage();
            ++usage.Model.NumberOfReposCreated;
            SaveUsage(usage);
        }

        public void IncrementPublishCount()
        {
            var usage = LoadUsage();
            ++usage.Model.NumberOfReposPublished;
            SaveUsage(usage);
        }

        public void IncrementOpenInGitHubCount()
        {
            var usage = LoadUsage();
            ++usage.Model.NumberOfOpenInGitHub;
            SaveUsage(usage);
        }

        public void IncrementLinkToGitHubCount()
        {
            var usage = LoadUsage();
            ++usage.Model.NumberOfLinkToGitHub;
            SaveUsage(usage);
        }

        public void IncrementCreateGistCount()
        {
            var usage = LoadUsage();
            ++usage.Model.NumberOfGists;
            SaveUsage(usage);
        }

        public void IncrementUpstreamPullRequestCount()
        {
            var usage = LoadUsage();
            ++usage.Model.NumberOfClones;
            SaveUsage(usage);
        }

        public void IncrementLoginCount()
        {
            var usage = LoadUsage();
            ++usage.Model.NumberOfLogins;
            SaveUsage(usage);
        }

        UsageStore LoadUsage()
        {
            var result = fileExists(storePath) ?
                SimpleJson.DeserializeObject<UsageStore>(readAllText(storePath, Encoding.UTF8)) :
                new UsageStore { Model = new UsageModel() };

            result.Model.Lang = CultureInfo.InstalledUICulture.IetfLanguageTag;
            result.Model.AppVersion = AssemblyVersionInformation.Version;
            result.Model.VSVersion = GitHub.VisualStudio.Services.VisualStudioVersion;

            return result;
        }

        void SaveUsage(UsageStore store)
        {
            dirCreate(System.IO.Path.GetDirectoryName(storePath));
            var json = SimpleJson.SerializeObject(store);
            writeAllText(storePath, json, Encoding.UTF8);
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
                var usage = LoadUsage();
                var lastDate = usage.LastUpdated;
                var currentDate = DateTimeOffset.Now;
                var includeWeekly = GetIso8601WeekOfYear(lastDate) != GetIso8601WeekOfYear(currentDate);
                var includeMonthly = lastDate.Month != currentDate.Month;

                // Only send stats once a day.
                if (lastDate.Date != currentDate.Date)
                {
                    await SendUsage(usage.Model, includeWeekly, includeMonthly);
                    ClearCounters(usage.Model, includeWeekly, includeMonthly);
                    usage.LastUpdated = DateTimeOffset.Now.UtcDateTime;
                    SaveUsage(usage);
                }
            }
            catch //(Exception ex)
            {
                //log.Warn("Failed submitting usage data", ex);
            }
        }

        async Task SendUsage(UsageModel usage, bool weekly, bool monthly)
        {
            Debug.Assert(client != null, "SendUsage should not be called when there is no IMetricsService");

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

        static void ClearCounters(UsageModel usage, bool weekly, bool monthly)
        {
            usage.NumberOfStartups = 0;
            usage.NumberOfClones = 0;
            usage.NumberOfReposCreated = 0;
            usage.NumberOfReposPublished = 0;
            usage.NumberOfGists = 0;
            usage.NumberOfOpenInGitHub = 0;
            usage.NumberOfLinkToGitHub = 0;
            usage.NumberOfLogins = 0;
            usage.NumberOfUpstreamPullRequests = 0;

            if (weekly)
                usage.NumberOfStartupsWeek = 0;

            if (monthly)
                usage.NumberOfStartupsMonth = 0;
        }

        class UsageStore
        {
            public DateTimeOffset LastUpdated { get; set; }
            public UsageModel Model { get; set; }
        }
    }
}
