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

        public async Task IncrementCounter(Expression<Func<UsageModel, int>> counter)
        {
            var usage = await LoadUsage();
            // because Model is a struct, it needs to be boxed in order for reflection to work
            object model = usage.Model;
            var field = (MemberExpression)counter.Body;
            var fieldInfo = (FieldInfo)field.Member;
            var value = (int)fieldInfo.GetValue(model);
            fieldInfo.SetValue(model, value + 1);
            usage.Model = (UsageModel)model;
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

        async Task IncrementLaunchCount()
        {
            var usage = await LoadUsage();
            var model = usage.Model;
            model.NumberOfStartups++;
            model.NumberOfStartupsWeek++;
            model.NumberOfStartupsMonth++;
            usage.Model = model;
            await service.WriteLocalData(usage);
        }

        async Task<UsageData> LoadUsage()
        {
            await Initialize();

            var usage = await service.ReadLocalData();
            var model = usage.Model;
            model.Lang = CultureInfo.InstalledUICulture.IetfLanguageTag;
            model.AppVersion = AssemblyVersionInformation.Version;
            model.VSVersion = vsservices.VSVersion;
            usage.Model = model;
            return usage;
        }

        async Task TimerTick()
        {
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
                usage.Model = UpdateModelUserData(usage.Model);

                await SendUsage(usage.Model, includeWeekly, includeMonthly);

                usage.Model = usage.Model.ClearCounters(includeWeekly, includeMonthly);
                usage.LastUpdated = DateTimeOffset.Now.UtcDateTime;
                await service.WriteLocalData(usage);
            }
        }

        async Task SendUsage(UsageModel usage, bool includeWeekly, bool includeMonthly)
        {
            if (client == null)
            {
                throw new GitHubLogicException("SendUsage should not be called when there is no IMetricsService");
            }

            var model = usage.Clone(includeWeekly, includeMonthly);
            await client.PostUsage(model);
        }

        UsageModel UpdateModelUserData(UsageModel model)
        {
            if (connectionManager.Connections.Any(x => x.HostAddress.IsGitHubDotCom()))
            {
                model.IsGitHubUser = true;
            }

            if (connectionManager.Connections.Any(x => !x.HostAddress.IsGitHubDotCom()))
            {
                model.IsEnterpriseUser = true;
            }
            return model;
        }
    }
}
