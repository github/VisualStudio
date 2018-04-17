using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GitHub.Helpers;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Settings;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    public sealed class UsageTracker : IUsageTracker, IDisposable
    {
        static readonly ILogger log = LogManager.ForContext<UsageTracker>();
        readonly IGitHubServiceProvider gitHubServiceProvider;

        bool initialized;
        IMetricsService metricsService;
        IUsageService dataService;
        IConnectionManager connectionManager;
        IPackageSettings packageSettings;
        IVSServices vsservices;
        IDisposable timer;
        bool firstTick = true;

        public UsageTracker(
            IGitHubServiceProvider gitHubServiceProvider,
            IUsageService service,
            IPackageSettings settings)
        {
            this.gitHubServiceProvider = gitHubServiceProvider;
            this.dataService = service;
            this.packageSettings = settings;
            timer = StartTimer();
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public async Task IncrementCounter(Expression<Func<UsageModel.MeasuresModel, int>> counter)
        {
            await Initialize();
            var data = await dataService.ReadUsageData();
            var usage = await GetCurrentReport(data);
            var property = (MemberExpression)counter.Body;
            var propertyInfo = (PropertyInfo)property.Member;
            log.Verbose("Increment counter {Name}", propertyInfo.Name);
            var value = (int)propertyInfo.GetValue(usage.Measures);
            propertyInfo.SetValue(usage.Measures, value + 1);
            await dataService.WriteUsageData(data);
        }

        IDisposable StartTimer()
        {
            return dataService.StartTimer(TimerTick, TimeSpan.FromMinutes(3), TimeSpan.FromDays(1));
        }

        async Task Initialize()
        {
            if (initialized)
                return;

            // The services needed by the usage tracker are loaded when they are first needed to
            // improve the startup time of the extension.
            await ThreadingHelper.SwitchToMainThreadAsync();

            metricsService = gitHubServiceProvider.TryGetService<IMetricsService>();
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

            var user = await dataService.ReadUserData();
            if (!user.SentOptIn)
            {
                try
                {
                    await metricsService.PostOptIn(packageSettings.CollectMetrics);
                    user.SentOptIn = true;
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to ping");
                }
                await dataService.WriteUserData(user);
            }

            if (!packageSettings.CollectMetrics)
            {
                return;
            }

            var data = await dataService.ReadUsageData();

            var changed = false;
            for (var i = data.Reports.Count - 1; i >= 0; --i)
            {
                var report = data.Reports[i];
                if (report.Dimensions.Date.Date != DateTimeOffset.Now.Date)
                {
                    try
                    {
                        await metricsService.PostUsage(report);
                        data.Reports.RemoveAt(i);
                        changed = true;
                    }
                    catch (Exception ex)
                    {
                        var date = report.Dimensions.Date.Date;
                        log.Error(ex, "Failed to send report for {Date}", date);
                    }
                }
            }

            if (changed)
            {
                await dataService.WriteUsageData(data);
            }
        }

        async Task<UsageModel> GetCurrentReport(UsageData data)
        {
            var current = data.Reports.FirstOrDefault(x => x.Dimensions.Date.Date == DateTimeOffset.Now.Date);

            if (current == null)
            {
                var user = await dataService.ReadUserData();
                current = UsageModel.Create(user.UserGuid);
                data.Reports.Add(current);
            }

            current.Dimensions.Lang = CultureInfo.InstalledUICulture.IetfLanguageTag;
            current.Dimensions.CurrentLang = CultureInfo.CurrentCulture.IetfLanguageTag;
            current.Dimensions.AppVersion = AssemblyVersionInformation.Version;
            current.Dimensions.VSVersion = vsservices.VSVersion;

            current.Dimensions.IsGitHubUser = connectionManager.Connections.Any(x => x.HostAddress.IsGitHubDotCom());
            current.Dimensions.IsEnterpriseUser = connectionManager.Connections.Any(x => !x.HostAddress.IsGitHubDotCom());
            return current;
        }
    }
}
