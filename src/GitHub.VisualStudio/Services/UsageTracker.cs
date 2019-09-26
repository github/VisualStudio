using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Settings;
using Microsoft.VisualStudio.Telemetry;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    public sealed class UsageTracker : IUsageTracker, IDisposable
    {
        private const int TelemetryVersion = 1; // Please update the version every time you want to indicate a change in telemetry logic when the extension itself is updated

        private const string EventNamePrefix = "vs/github/usagetracker/";
        private const string PropertyPrefix = "vs.github.";

        static class Event
        {
            public const string UsageTracker = EventNamePrefix + "increment-counter";
        }

        static class Property
        {
            public const string TelemetryVersion = PropertyPrefix + nameof(TelemetryVersion);
            public const string CounterName = PropertyPrefix + nameof(CounterName);
            public const string ExtensionVersion = PropertyPrefix + nameof(ExtensionVersion);
            public const string IsGitHubUser = PropertyPrefix + nameof(IsGitHubUser);
            public const string IsEnterpriseUser = PropertyPrefix + nameof(IsEnterpriseUser);
        }

        static readonly ILogger log = LogManager.ForContext<UsageTracker>();
        readonly IGitHubServiceProvider gitHubServiceProvider;

        bool initialized;
        IMetricsService client;
        readonly IUsageService service;
        IConnectionManager connectionManager;
        readonly IPackageSettings userSettings;
        IVSServices vsservices;
        IDisposable timer;
        bool firstTick = true;

        public UsageTracker(
            IGitHubServiceProvider gitHubServiceProvider,
            IUsageService service,
            IPackageSettings settings,
            JoinableTaskContext joinableTaskContext)
        {
            this.gitHubServiceProvider = gitHubServiceProvider;
            this.service = service;
            this.userSettings = settings;
            JoinableTaskContext = joinableTaskContext;
            timer = StartTimer();
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public async Task IncrementCounter(Expression<Func<UsageModel.MeasuresModel, int>> counter)
        {
            await Initialize();

            var property = (MemberExpression)counter.Body;
            var propertyInfo = (PropertyInfo)property.Member;
            var counterName = propertyInfo.Name;
            log.Verbose("Increment counter {Name}", counterName);

            var updateTask = UpdateUsageMetrics(propertyInfo);

            LogTelemetryEvent(counterName);

            await updateTask;
        }

        void LogTelemetryEvent(string counterName)
        {
            const string numberOfPrefix = "numberof";
            if (counterName.StartsWith(numberOfPrefix, StringComparison.OrdinalIgnoreCase))
            {
                counterName = counterName.Substring(numberOfPrefix.Length);
            }

            var operation = new TelemetryEvent(Event.UsageTracker);
            operation.Properties[Property.TelemetryVersion] = TelemetryVersion;
            operation.Properties[Property.CounterName] = counterName;
            operation.Properties[Property.ExtensionVersion] = AssemblyVersionInformation.Version;
            operation.Properties[Property.IsGitHubUser] = IsGitHubUser;
            operation.Properties[Property.IsEnterpriseUser] = IsEnterpriseUser;

            TelemetryService.DefaultSession.PostEvent(operation);
        }

        bool IsEnterpriseUser =>
            this.connectionManager?.Connections.Any(x => !x.HostAddress.IsGitHubDotCom()) ?? false;

        bool IsGitHubUser =>
            this.connectionManager?.Connections.Any(x => x.HostAddress.IsGitHubDotCom()) ?? false;

        async Task UpdateUsageMetrics(PropertyInfo propertyInfo)
        {
            var data = await service.ReadLocalData();
            var usage = await GetCurrentReport(data);

            var value = (int) propertyInfo.GetValue(usage.Measures);
            propertyInfo.SetValue(usage.Measures, value + 1);

            await service.WriteLocalData(data);
        }

        IDisposable StartTimer()
        {
            return service.StartTimer(TimerTick, TimeSpan.FromMinutes(3), TimeSpan.FromDays(1));
        }

        async Task Initialize()
        {
            if (initialized)
                return;

            // The services needed by the usage tracker are loaded when they are first needed to
            // improve the startup time of the extension.
            await JoinableTaskContext.Factory.SwitchToMainThreadAsync();

            client = gitHubServiceProvider.TryGetService<IMetricsService>();
            connectionManager = gitHubServiceProvider.GetService<IConnectionManager>();
            vsservices = gitHubServiceProvider.GetService<IVSServices>();
            initialized = true;
        }

        async Task TimerTick()
        {
            await Initialize();

            if (firstTick)
            {
                await IncrementCounter(x => x.NumberOfStartups);
                firstTick = false;
            }

            if (client == null || !userSettings.CollectMetrics)
            {
                timer.Dispose();
                timer = null;
                return;
            }

            var data = await service.ReadLocalData();

            var changed = false;
            for (var i = data.Reports.Count - 1; i >= 0; --i)
            {
                if (data.Reports[i].Dimensions.Date.Date != DateTimeOffset.Now.Date)
                {
                    try
                    {
                        await client.PostUsage(data.Reports[i]);
                        data.Reports.RemoveAt(i);
                        changed = true;
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Failed to send metrics");
                    }
                }
            }

            if (changed)
            {
                await service.WriteLocalData(data);
            }
        }

        async Task<UsageModel> GetCurrentReport(UsageData data)
        {
            var current = data.Reports.FirstOrDefault(x => x.Dimensions.Date.Date == DateTimeOffset.Now.Date);

            if (current == null)
            {
                var guid = await service.GetUserGuid();
                current = UsageModel.Create(guid);
                data.Reports.Add(current);
            }

            current.Dimensions.Lang = CultureInfo.InstalledUICulture.IetfLanguageTag;
            current.Dimensions.CurrentLang = CultureInfo.CurrentCulture.IetfLanguageTag;
            current.Dimensions.CurrentUILang = CultureInfo.CurrentUICulture.IetfLanguageTag;
            current.Dimensions.AppVersion = AssemblyVersionInformation.Version;
            current.Dimensions.VSVersion = vsservices.VSVersion;

            current.Dimensions.IsGitHubUser = IsGitHubUser;
            current.Dimensions.IsEnterpriseUser = IsEnterpriseUser;
            return current;
        }

        JoinableTaskContext JoinableTaskContext { get; }
    }
}
