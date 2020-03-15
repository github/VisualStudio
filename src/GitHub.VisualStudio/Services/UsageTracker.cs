using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Settings;
using Microsoft.VisualStudio.Threading;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    public sealed class UsageTracker : IUsageTracker, IDisposable
    {
        static readonly ILogger log = LogManager.ForContext<UsageTracker>();
        readonly IGitHubServiceProvider gitHubServiceProvider;

        bool initialized;
        IMetricsService client;
        readonly IUsageService service;
        Lazy<IConnectionManager> connectionManager;
        readonly IPackageSettings userSettings;
        readonly bool vsTelemetry;
        IVSServices vsservices;
        IUsageTracker visualStudioUsageTracker;
        IDisposable timer;
        bool firstTick = true;

        public UsageTracker(
            IGitHubServiceProvider gitHubServiceProvider,
            IUsageService service,
            IPackageSettings settings,
            JoinableTaskContext joinableTaskContext,
            bool vsTelemetry)
        {
            this.gitHubServiceProvider = gitHubServiceProvider;
            this.service = service;
            this.userSettings = settings;
            JoinableTaskContext = joinableTaskContext;
            this.vsTelemetry = vsTelemetry;

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

            if (visualStudioUsageTracker != null)
            {
                // Not available on Visual Studio 2015
                await visualStudioUsageTracker.IncrementCounter(counter);
            }

            await updateTask;
        }

        bool IsEnterpriseUser =>
            connectionManager.Value?.Connections.Any(x => !x.HostAddress.IsGitHubDotCom()) ?? false;

        bool IsGitHubUser =>
            connectionManager.Value?.Connections.Any(x => x.HostAddress.IsGitHubDotCom()) ?? false;

        async Task UpdateUsageMetrics(PropertyInfo propertyInfo)
        {
            var data = await service.ReadLocalData();
            var usage = await GetCurrentReport(data);

            var value = (int)propertyInfo.GetValue(usage.Measures);
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
            connectionManager = new Lazy<IConnectionManager>(() => gitHubServiceProvider.GetService<IConnectionManager>());
            vsservices = gitHubServiceProvider.GetService<IVSServices>();

            if (vsTelemetry)
            {
                log.Verbose("Creating VisualStudioUsageTracker");
                visualStudioUsageTracker = new VisualStudioUsageTracker(connectionManager);
            }

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
            current.Dimensions.AppVersion = ExtensionInformation.Version;
            current.Dimensions.VSVersion = vsservices.VSVersion;

            current.Dimensions.IsGitHubUser = IsGitHubUser;
            current.Dimensions.IsEnterpriseUser = IsEnterpriseUser;
            return current;
        }

        JoinableTaskContext JoinableTaskContext { get; }
    }
}
