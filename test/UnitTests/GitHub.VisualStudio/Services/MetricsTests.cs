using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using System.Globalization;
using GitHub.Reflection;

namespace MetricsTests
{
    public class UsageTrackerTests : TestBaseClass
    {
        [Test]
        public void ShouldStartTimer()
        {
            var serviceProvider = CreateServiceProvider();
            var usageService = serviceProvider.GetService<IUsageService>();
            var target = new UsageTracker(serviceProvider, usageService);
            usageService.Received(1).StartTimer(Arg.Any<Func<Task>>(), TimeSpan.FromMinutes(3), TimeSpan.FromHours(8));
        }

        [Test]
        public async Task FirstTickShouldIncrementLaunchCount()
        {
            var usageService = CreateUsageService();
            var tick = GetTick(usageService);

            await tick();

            await usageService.Received(1).WriteLocalData(
                Arg.Is<UsageData>(x =>
                    x.Model.NumberOfStartups == 1 &&
                    x.Model.NumberOfStartupsWeek == 1 &&
                    x.Model.NumberOfStartupsMonth == 1));
        }

        [Test]
        public async Task SubsequentTickShouldNotIncrementLaunchCount()
        {
            var usageService = CreateUsageService();
            var tick = GetTick(usageService);

            await tick();
            usageService.ClearReceivedCalls();
            await tick();

            await usageService.DidNotReceiveWithAnyArgs().WriteLocalData(null);
        }

        [Test]
        public async Task ShouldDisposeTimerIfMetricsServiceNotFound()
        {
            var usageService = CreateUsageService();
            var disposed = false;
            var disposable = Disposable.Create(() => disposed = true);
            usageService.StartTimer(null, new TimeSpan(), new TimeSpan()).ReturnsForAnyArgs(disposable);

            var tick = GetTick(usageService);

            await tick();

            Assert.True(disposed);
        }

        [Test]
        public async Task TickShouldNotSendDataIfSameDay()
        {
            var usageService = CreateUsageService();
            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            await tick();

            var metricsService = serviceProvider.TryGetService<IMetricsService>();
            await metricsService.DidNotReceive().PostUsage(Arg.Any<UsageModel>());
        }

        [Test]
        public async Task TickShouldSendDataIfDifferentDay()
        {
            var usageService = CreateUsageService(sameDay: false);
            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            await tick();

            var metricsService = serviceProvider.TryGetService<IMetricsService>();
            await metricsService.Received(1).PostUsage(Arg.Any<UsageModel>());
        }

        [Test]
        public async Task NonWeeklyOrMonthlyCountersShouldBeZeroed()
        {
            var usageService = CreateUsageService(new UsageModel
            {
                NumberOfStartups = 1,
                NumberOfStartupsWeek = 1,
                NumberOfStartupsMonth = 1,
                NumberOfClones = 1,
            }, sameDay: false);

            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            await tick();

            await usageService.Received().WriteLocalData(
                Arg.Is<UsageData>(x =>
                    x.Model.NumberOfStartups == 0 &&
                    x.Model.NumberOfStartupsWeek == 2 &&
                    x.Model.NumberOfStartupsMonth == 2 &&
                    x.Model.NumberOfClones == 0));
        }

        [Test]
        public async Task NonMonthlyCountersShouldBeZeroed()
        {
            var usageService = CreateUsageService(new UsageModel
            {
                NumberOfStartups = 1,
                NumberOfStartupsWeek = 1,
                NumberOfStartupsMonth = 1,
                NumberOfClones = 1,
            }, sameDay: false, sameWeek: false);

            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            await tick();

            await usageService.Received().WriteLocalData(
                Arg.Is<UsageData>(x =>
                    x.Model.NumberOfStartups == 0 &&
                    x.Model.NumberOfStartupsWeek == 0 &&
                    x.Model.NumberOfStartupsMonth == 2 &&
                    x.Model.NumberOfClones == 0));
        }

        [Test]
        public async Task AllCountersShouldBeZeroed()
        {
            var usageService = CreateUsageService(new UsageModel
            {
                NumberOfStartups = 1,
                NumberOfStartupsWeek = 1,
                NumberOfStartupsMonth = 1,
                NumberOfClones = 1,
            }, sameDay: false, sameWeek: false, sameMonth: false);

            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            await tick();

            await usageService.Received().WriteLocalData(
                Arg.Is<UsageData>(x =>
                    x.Model.NumberOfStartups == 0 &&
                    x.Model.NumberOfStartupsWeek == 0 &&
                    x.Model.NumberOfStartupsMonth == 0 &&
                    x.Model.NumberOfClones == 0));
        }

        [Test]
        public async Task ShouldIncrementCounter()
        {
            var model = new UsageModel { NumberOfClones = 4 };
            var usageService = CreateUsageService(model);
            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            await usageTracker.IncrementCounter(x => x.NumberOfClones);
            UsageData result = usageService.ReceivedCalls().First(x => x.GetMethodInfo().Name == "WriteLocalData").GetArguments()[0] as UsageData;

            Assert.AreEqual(5, result.Model.NumberOfClones);
        }

        [Test]
        public async Task ShouldWriteUpdatedData()
        {
            var data = new UsageData { Model = new UsageModel() };
            var usageService = CreateUsageService(data);
            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            await usageTracker.IncrementCounter(x => x.NumberOfClones);

            await usageService.Received(1).WriteLocalData(data);
        }

        [Test]
        public async Task UsageServiceWritesAllTheDataCorrectly()
        {
            var model = CreateUsageModel();

            var usageService = CreateUsageService(model, sameDay: true);
            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            var vsservices = serviceProvider.GetService<IVSServices>();
            vsservices.VSVersion.Returns(model.VSVersion);

            await tick();

            var metricsService = serviceProvider.TryGetService<IMetricsService>();

            var expected = model;
            expected.NumberOfStartups++;
            expected.NumberOfStartupsWeek++;
            expected.NumberOfStartupsMonth++;

            var result = (usageService.ReceivedCalls().First(x => x.GetMethodInfo().Name == "WriteLocalData").GetArguments()[0] as UsageData).Model;
            CollectionAssert.AreEquivalent(
                ReflectionUtils.GetProperties(expected.GetType()).Select(x => new { x.Name, Value = x.GetValue(expected) }),
                ReflectionUtils.GetProperties(result.GetType()).Select(x => new { x.Name, Value = x.GetValue(result) }));
        }

        [Test]
        public async Task MetricsServiceSendsDailyData()
        {
            var model = CreateUsageModel();

            var usageService = CreateUsageService(model, sameDay: false);
            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            var vsservices = serviceProvider.GetService<IVSServices>();
            vsservices.VSVersion.Returns(model.VSVersion);

            await tick();

            var metricsService = serviceProvider.TryGetService<IMetricsService>();
            var list = metricsService.ReceivedCalls().Select(x => x.GetMethodInfo().Name);
            var result = (UsageModel)metricsService.ReceivedCalls().First(x => x.GetMethodInfo().Name == "PostUsage").GetArguments()[0];

            var expected = model.Clone(false, false);
            expected.NumberOfStartups++;

            CollectionAssert.AreEquivalent(
                ReflectionUtils.GetProperties(expected.GetType()).Select(x => new { x.Name, Value = x.GetValue(expected) }),
                ReflectionUtils.GetProperties(result.GetType()).Select(x => new { x.Name, Value = x.GetValue(result) }));
        }

        [Test]
        public async Task MetricsServiceSendsWeeklyData()
        {
            var model = CreateUsageModel();

            var usageService = CreateUsageService(model, sameDay: false, sameWeek: false);
            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            var vsservices = serviceProvider.GetService<IVSServices>();
            vsservices.VSVersion.Returns(model.VSVersion);

            await tick();

            var metricsService = serviceProvider.TryGetService<IMetricsService>();
            var list = metricsService.ReceivedCalls().Select(x => x.GetMethodInfo().Name);
            var result = (UsageModel)metricsService.ReceivedCalls().First(x => x.GetMethodInfo().Name == "PostUsage").GetArguments()[0];

            var expected = model.Clone(true, false);
            expected.NumberOfStartups++;
            expected.NumberOfStartupsWeek++;

            CollectionAssert.AreEquivalent(
                ReflectionUtils.GetProperties(expected.GetType()).Select(x => new { x.Name, Value = x.GetValue(expected) }),
                ReflectionUtils.GetProperties(result.GetType()).Select(x => new { x.Name, Value = x.GetValue(result) }));
        }

        [Test]
        public async Task MetricsServiceSendsMonthlyData()
        {
            var model = CreateUsageModel();

            var usageService = CreateUsageService(model, sameDay: false, sameWeek: false, sameMonth: false);
            var serviceProvider = CreateServiceProvider(usageService);
            var usageTracker = new UsageTracker(serviceProvider, usageService);
            var tick = GetTick(usageService, usageTracker);

            var vsservices = serviceProvider.GetService<IVSServices>();
            vsservices.VSVersion.Returns(model.VSVersion);

            await tick();

            var metricsService = serviceProvider.TryGetService<IMetricsService>();
            var list = metricsService.ReceivedCalls().Select(x => x.GetMethodInfo().Name);
            var result = (UsageModel)metricsService.ReceivedCalls().First(x => x.GetMethodInfo().Name == "PostUsage").GetArguments()[0];

            var expected = model;
            expected.NumberOfStartups++;
            expected.NumberOfStartupsWeek++;
            expected.NumberOfStartupsMonth++;

            CollectionAssert.AreEquivalent(
                ReflectionUtils.GetProperties(expected.GetType()).Select(x => new { x.Name, Value = x.GetValue(expected) }),
                ReflectionUtils.GetProperties(result.GetType()).Select(x => new { x.Name, Value = x.GetValue(result) }));
        }

        static UsageModel CreateUsageModel()
        {
            var count = 1;
            // UsageModel is a struct so we have to force box it to be able to set values on it
            object model = new UsageModel();
            var props = ReflectionUtils.GetProperties(model.GetType());
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(model, count++);
                }
                else if (prop.PropertyType == typeof(string))
                {
                    if (prop.Name == "Lang")
                        prop.SetValue(model, CultureInfo.InstalledUICulture.IetfLanguageTag);
                    else if (prop.Name == "AppVersion")
                        prop.SetValue(model, AssemblyVersionInformation.Version);
                    else
                        prop.SetValue(model, $"string {count++}");
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(model, true);
                }
                else
                    Assert.Fail("Unknown field type in UsageModel. Fix this test to support it");
            }

            return (UsageModel)model;
        }

        static Func<Task> GetTick(IUsageService service, IUsageTracker usageTracker = null)
        {
            Func<Task> tick = null;

            service.WhenForAnyArgs(x => x.StartTimer(null, new TimeSpan(), new TimeSpan()))
                .Do(x => tick = x.ArgAt<Func<Task>>(0));

            if (usageTracker == null)
                usageTracker = new UsageTracker(CreateServiceProvider(), service);

            return tick;
        }

        static IGitHubServiceProvider CreateServiceProvider(IUsageService usageService = null)
        {
            return CreateServiceProvider(Substitute.For<IMetricsService>(), usageService);
        }

        static IGitHubServiceProvider CreateServiceProvider(IMetricsService metricsService, IUsageService usageService = null)
        {
            var result = Substitute.For<IGitHubServiceProvider>();
            var connectionManager = Substitute.For<IConnectionManager>();
            var packageSettings = Substitute.For<IPackageSettings>();
            var vsservices = Substitute.For<IVSServices>();

            connectionManager.Connections.Returns(new ObservableCollectionEx<IConnection>());
            packageSettings.CollectMetrics.Returns(true);

            result.GetService<IConnectionManager>().Returns(connectionManager);
            result.GetService<IPackageSettings>().Returns(packageSettings);
            result.GetService<IVSServices>().Returns(vsservices);
            result.TryGetService<IMetricsService>().Returns(metricsService);
            result.GetService<IUsageService>().Returns(usageService ?? CreateUsageService());

            return result;
        }

        static IUsageService CreateUsageService(
            bool sameDay = true,
            bool sameWeek = true,
            bool sameMonth = true)
        {
            return CreateUsageService(new UsageModel(), sameDay, sameWeek, sameMonth);
        }

        static IUsageService CreateUsageService(
            UsageModel model,
            bool sameDay = true,
            bool sameWeek = true,
            bool sameMonth = true)
        {
            return CreateUsageService(new UsageData
            {
                LastUpdated = DateTimeOffset.Now,
                Model = model
            }, sameDay, sameWeek, sameMonth);
        }

        static IUsageService CreateUsageService(
            UsageData data,
            bool sameDay = true,
            bool sameWeek = true,
            bool sameMonth = true)
        {
            var result = Substitute.For<IUsageService>();
            result.ReadLocalData().Returns(data);
            result.IsSameDay(DateTimeOffset.Now).ReturnsForAnyArgs(sameDay);
            result.IsSameWeek(DateTimeOffset.Now).ReturnsForAnyArgs(sameWeek);
            result.IsSameMonth(DateTimeOffset.Now).ReturnsForAnyArgs(sameMonth);
            return result;
        }
    }

    public class UsageServiceTests : TestBaseClass
    {
        [Test]
        public void IsSameDayWorks()
        {
            var usageService = new UsageService(Substitute.For<IGitHubServiceProvider>());
            var now = DateTimeOffset.Now;
            Assert.True(usageService.IsSameDay(now));
            Assert.True(usageService.IsSameDay(new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero)));
            Assert.False(usageService.IsSameDay(new DateTimeOffset(now.Year, now.Month, now.Day+1, 0, 0, 0, TimeSpan.Zero)));
            Assert.False(usageService.IsSameDay(new DateTimeOffset(now.Year, now.Month, now.Day-1, 0, 0, 0, TimeSpan.Zero)));
            Assert.True(usageService.IsSameDay(new DateTimeOffset(now.Year, now.Month, now.Day, 10, 3, 1, TimeSpan.Zero)));
            Assert.False(usageService.IsSameDay(new DateTimeOffset(now.Year, now.Month, now.Day+1, 10, 3, 1, TimeSpan.Zero)));
            Assert.False(usageService.IsSameDay(new DateTimeOffset(now.Year, now.Month, now.Day-1, 10, 3, 1, TimeSpan.Zero)));
        }

        [Test]
        public void IsSameWeekWorks()
        {
            var usageService = new UsageService(Substitute.For<IGitHubServiceProvider>());
            var now = DateTimeOffset.Now;

            Assert.True(usageService.IsSameWeek(now));
            var nowWeek = GetIso8601WeekOfYear(now);

            DateTimeOffset nextWeek = now;
            for (int i = 1; i < 8; i++)
            {
                nextWeek = nextWeek.AddDays(1);
                var week = GetIso8601WeekOfYear(nextWeek);
                Assert.AreEqual(week == nowWeek, usageService.IsSameWeek(nextWeek));
            }

            DateTimeOffset prevWeek = now;
            for (int i = 1; i < 8; i++)
            {
                prevWeek = prevWeek.AddDays(-1);
                var week = GetIso8601WeekOfYear(prevWeek);
                Assert.AreEqual(week == nowWeek, usageService.IsSameWeek(prevWeek));
            }

            Assert.False(usageService.IsSameWeek(now.AddYears(1)));
        }

        [Test]
        public void IsSameMonthWorks()
        {
            var usageService = new UsageService(Substitute.For<IGitHubServiceProvider>());
            var now = DateTimeOffset.Now;

            Assert.True(usageService.IsSameMonth(now));

            DateTimeOffset nextMonth = now;
            for (int i = 1; i < 40; i++)
            {
                nextMonth = nextMonth.AddDays(1);
                Assert.AreEqual(nextMonth.Month == now.Month, usageService.IsSameMonth(nextMonth));
            }

            DateTimeOffset prevMonth = now;
            for (int i = 1; i < 40; i++)
            {
                prevMonth = prevMonth.AddDays(-1);
                Assert.AreEqual(prevMonth.Month == now.Month, usageService.IsSameMonth(prevMonth));
            }

            Assert.False(usageService.IsSameMonth(now.AddYears(1)));
        }

        // http://blogs.msdn.com/b/shawnste/archive/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net.aspx
        static int GetIso8601WeekOfYear(DateTimeOffset time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time.UtcDateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time.UtcDateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}
