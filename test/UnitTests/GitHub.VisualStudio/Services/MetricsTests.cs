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
using System.IO;
using GitHub.Reflection;
using Rothko;

namespace MetricsTests
{
    public class UsageTrackerTests : TestBaseClass
    {
        [Test]
        public void ShouldStartTimer()
        {
            var service = Substitute.For<IUsageService>();
            var target = new UsageTracker(CreateServiceProvider(), service);

            service.Received(1).StartTimer(Arg.Any<Func<Task>>(), TimeSpan.FromMinutes(3), TimeSpan.FromHours(8));
        }

        [Test]
        public async Task FirstTickShouldIncrementLaunchCount()
        {
            var service = CreateUsageService();
            var targetAndTick = CreateTargetAndGetTick(CreateServiceProvider(), service);

            await targetAndTick.Item2();

            await service.Received(1).WriteLocalData(
                Arg.Is<UsageData>(x =>
                    x.Model.NumberOfStartups == 1 &&
                    x.Model.NumberOfStartupsWeek == 1 &&
                    x.Model.NumberOfStartupsMonth == 1));
        }

        [Test]
        public async Task SubsequentTickShouldNotIncrementLaunchCount()
        {
            var service = CreateUsageService();
            var targetAndTick = CreateTargetAndGetTick(CreateServiceProvider(), service);

            await targetAndTick.Item2();
            service.ClearReceivedCalls();
            await targetAndTick.Item2();

            await service.DidNotReceiveWithAnyArgs().WriteLocalData(null);
        }

        [Test]
        public async Task ShouldDisposeTimerIfMetricsServiceNotFound()
        {
            var service = CreateUsageService();
            var disposed = false;
            var disposable = Disposable.Create(() => disposed = true);
            service.StartTimer(null, new TimeSpan(), new TimeSpan()).ReturnsForAnyArgs(disposable);

            var targetAndTick = CreateTargetAndGetTick(
                CreateServiceProvider(hasMetricsService: false),
                service);

            await targetAndTick.Item2();

            Assert.True(disposed);
        }

        [Test]
        public async Task TickShouldNotSendDataIfSameDay()
        {
            var serviceProvider = CreateServiceProvider();
            var targetAndTick = CreateTargetAndGetTick(
                serviceProvider,
                CreateUsageService());

            await targetAndTick.Item2();

            var metricsService = serviceProvider.TryGetService<IMetricsService>();
            await metricsService.DidNotReceive().PostUsage(Arg.Any<UsageModel>());
        }

        [Test]
        public async Task TickShouldSendDataIfDifferentDay()
        {
            var serviceProvider = CreateServiceProvider();
            var targetAndTick = CreateTargetAndGetTick(
                serviceProvider,
                CreateUsageService(sameDay: false));

            await targetAndTick.Item2();

            var metricsService = serviceProvider.TryGetService<IMetricsService>();
            await metricsService.Received(1).PostUsage(Arg.Any<UsageModel>());
        }

        [Test]
        public async Task NonWeeklyOrMonthlyCountersShouldBeZeroed()
        {
            var service = CreateUsageService(new UsageModel
            {
                NumberOfStartups = 1,
                NumberOfStartupsWeek = 1,
                NumberOfStartupsMonth = 1,
                NumberOfClones = 1,
            }, sameDay: false);
            Func<Task> tick = null;

            service.WhenForAnyArgs(x => x.StartTimer(null, new TimeSpan(), new TimeSpan()))
                .Do(x => tick = x.ArgAt<Func<Task>>(0));

            var serviceProvider = CreateServiceProvider();
            var target = new UsageTracker(serviceProvider, service);

            await tick();

            await service.Received().WriteLocalData(
                Arg.Is<UsageData>(x =>
                    x.Model.NumberOfStartups == 0 &&
                    x.Model.NumberOfStartupsWeek == 2 &&
                    x.Model.NumberOfStartupsMonth == 2 &&
                    x.Model.NumberOfClones == 0));
        }

        [Test]
        public async Task NonMonthlyCountersShouldBeZeroed()
        {
            var service = CreateUsageService(new UsageModel
            {
                NumberOfStartups = 1,
                NumberOfStartupsWeek = 1,
                NumberOfStartupsMonth = 1,
                NumberOfClones = 1,
            }, sameDay: false, sameWeek: false);
            Func<Task> tick = null;

            service.WhenForAnyArgs(x => x.StartTimer(null, new TimeSpan(), new TimeSpan()))
                .Do(x => tick = x.ArgAt<Func<Task>>(0));

            var serviceProvider = CreateServiceProvider();
            var target = new UsageTracker(serviceProvider, service);

            await tick();

            await service.Received().WriteLocalData(
                Arg.Is<UsageData>(x =>
                    x.Model.NumberOfStartups == 0 &&
                    x.Model.NumberOfStartupsWeek == 0 &&
                    x.Model.NumberOfStartupsMonth == 2 &&
                    x.Model.NumberOfClones == 0));
        }

        [Test]
        public async Task AllCountersShouldBeZeroed()
        {
            var service = CreateUsageService(new UsageModel
            {
                NumberOfStartups = 1,
                NumberOfStartupsWeek = 1,
                NumberOfStartupsMonth = 1,
                NumberOfClones = 1,
            }, sameDay: false, sameWeek: false, sameMonth: false);
            Func<Task> tick = null;

            service.WhenForAnyArgs(x => x.StartTimer(null, new TimeSpan(), new TimeSpan()))
                .Do(x => tick = x.ArgAt<Func<Task>>(0));

            var serviceProvider = CreateServiceProvider();
            var target = new UsageTracker(serviceProvider, service);

            await tick();

            await service.Received().WriteLocalData(
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
            var target = new UsageTracker(
                CreateServiceProvider(),
                usageService);

            await target.IncrementCounter(x => x.NumberOfClones);
            UsageData result = usageService.ReceivedCalls().First(x => x.GetMethodInfo().Name == "WriteLocalData").GetArguments()[0] as UsageData;

            Assert.AreEqual(5, result.Model.NumberOfClones);
        }

        [Test]
        public async Task ShouldWriteUpdatedData()
        {
            var data = new UsageData { Model = new UsageModel() };
            var service = CreateUsageService(data);
            var target = new UsageTracker(
                CreateServiceProvider(),
                service);

            await target.IncrementCounter(x => x.NumberOfClones);

            await service.Received(1).WriteLocalData(data);
        }

        [Test]
        public async Task UsageServiceWritesAllTheDataCorrectly()
        {
            var model = CreateUsageModel();
            var serviceProvider = CreateServiceProvider();
            var usageService = CreateUsageService(model, sameDay: true);
            var targetAndTick = CreateTargetAndGetTick(serviceProvider, usageService);
            var vsservices = serviceProvider.GetService<IVSServices>();
            vsservices.VSVersion.Returns(model.VSVersion);

            await targetAndTick.Item2();

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
            var serviceProvider = CreateServiceProvider();
            var vsservices = serviceProvider.GetService<IVSServices>();
            vsservices.VSVersion.Returns(model.VSVersion);

            var targetAndTick = CreateTargetAndGetTick(
                serviceProvider,
                CreateUsageService(model, sameDay: false));

            await targetAndTick.Item2();

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
            var serviceProvider = CreateServiceProvider();
            var vsservices = serviceProvider.GetService<IVSServices>();
            vsservices.VSVersion.Returns(model.VSVersion);

            var targetAndTick = CreateTargetAndGetTick(
                serviceProvider,
                CreateUsageService(model, sameDay: false, sameWeek: false));

            await targetAndTick.Item2();

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
            var serviceProvider = CreateServiceProvider();
            var vsservices = serviceProvider.GetService<IVSServices>();
            vsservices.VSVersion.Returns(model.VSVersion);

            var targetAndTick = CreateTargetAndGetTick(
                serviceProvider,
                CreateUsageService(model, sameDay: false, sameWeek: false, sameMonth: false));

            await targetAndTick.Item2();

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
                else if (prop.PropertyType == typeof(Guid))
                {
                    prop.SetValue(model, Guid.Empty);
                }
                else
                    Assert.Fail("Unknown field type in UsageModel. Fix this test to support it");
            }

            return (UsageModel)model;
        }

        static Tuple<UsageTracker, Func<Task>> CreateTargetAndGetTick(
            IGitHubServiceProvider serviceProvider,
            IUsageService service)
        {
            Func<Task> tick = null;

            service.WhenForAnyArgs(x => x.StartTimer(null, new TimeSpan(), new TimeSpan()))
                .Do(x => tick = x.ArgAt<Func<Task>>(0));

            var target = new UsageTracker(serviceProvider, service);

            return Tuple.Create(target, tick);
        }

        static IGitHubServiceProvider CreateServiceProvider(bool hasMetricsService = true)
        {
            var result = Substitute.For<IGitHubServiceProvider>();
            var connectionManager = Substitute.For<IConnectionManager>();
            var metricsService = Substitute.For<IMetricsService>();
            var packageSettings = Substitute.For<IPackageSettings>();
            var vsservices = Substitute.For<IVSServices>();

            connectionManager.Connections.Returns(new ObservableCollectionEx<IConnection>());
            packageSettings.CollectMetrics.Returns(true);

            result.GetService<IConnectionManager>().Returns(connectionManager);
            result.GetService<IPackageSettings>().Returns(packageSettings);
            result.GetService<IVSServices>().Returns(vsservices);
            result.TryGetService<IMetricsService>().Returns(hasMetricsService ? metricsService : null);

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
        private static readonly Guid UserGuid = Guid.NewGuid();
        private static readonly string DefaultUserStoreContent = @"{""UserGuid"":""" + UserGuid + @"""}";

        private static readonly string DefaultUsageContent = string.Empty;

        private static readonly string LegacyUsageContent =
@"{
	""LastUpdated"": ""2018-02-28T12:37:09.4771538Z"",
	""Model"": {
		""IsGitHubUser"": true,
		""IsEnterpriseUser"": false,
		""AppVersion"": ""2.4.3.0"",
		""VSVersion"": ""14.0.25431.01 Update 3"",
		""Lang"": ""en-US"",
		""NumberOfStartups"": 0,
		""NumberOfStartupsWeek"": 3,
		""NumberOfStartupsMonth"": 23,
		""NumberOfUpstreamPullRequests"": 0,
		""NumberOfClones"": 0,
		""NumberOfReposCreated"": 0,
		""NumberOfReposPublished"": 0,
		""NumberOfGists"": 0,
		""NumberOfOpenInGitHub"": 0,
		""NumberOfLinkToGitHub"": 2,
		""NumberOfLogins"": 1,
		""NumberOfOAuthLogins"": 0,
		""NumberOfTokenLogins"": 0,
		""NumberOfPullRequestsOpened"": 1,
		""NumberOfLocalPullRequestsCheckedOut"": 0,
		""NumberOfLocalPullRequestPulls"": 0,
		""NumberOfLocalPullRequestPushes"": 0,
		""NumberOfForkPullRequestsCheckedOut"": 0,
		""NumberOfForkPullRequestPulls"": 0,
		""NumberOfForkPullRequestPushes"": 0,
		""NumberOfSyncSubmodules"": 0,
		""NumberOfWelcomeDocsClicks"": 0,
		""NumberOfWelcomeTrainingClicks"": 0,
		""NumberOfGitHubPaneHelpClicks"": 0,
		""NumberOfPRDetailsViewChanges"": 1,
		""NumberOfPRDetailsViewFile"": 0,
		""NumberOfPRDetailsCompareWithSolution"": 0,
		""NumberOfPRDetailsOpenFileInSolution"": 0,
		""NumberOfPRDetailsNavigateToEditor"": 0,
		""NumberOfPRReviewDiffViewInlineCommentOpen"": 1,
		""NumberOfPRReviewDiffViewInlineCommentPost"": 0,
		""NumberOfShowCurrentPullRequest"": 2
	}
}";

        private string storeFileName;
        private string userFileName;
        private string localApplicationDataPath;
        private IEnvironment environment;

        [SetUp]
        public void SetUp()
        {
            localApplicationDataPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            if (File.Exists(localApplicationDataPath))
            {
                File.Delete(localApplicationDataPath);
            }

            if (Directory.Exists(localApplicationDataPath))
            {
                Directory.Delete(localApplicationDataPath);
            }

            Directory.CreateDirectory(localApplicationDataPath);

            storeFileName = Path.Combine(localApplicationDataPath, "ghfvs.usage");
            userFileName = Path.Combine(localApplicationDataPath, "user.json");

            environment = Substitute.For<IEnvironment>();
            environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(localApplicationDataPath);

            WriteUsageFileContent(DefaultUsageContent);
            WriteUserFileContent(DefaultUserStoreContent);
        }

        private void WriteUsageFileContent(string content)
        {
            File.WriteAllText(storeFileName, content);
        }
        private void WriteUserFileContent(string content)
        {
            File.WriteAllText(userFileName, content);
        }

        [Test]
        public async Task GetUserGuidWorks()
        {
            var usageService = new UsageService(Substitute.For<IGitHubServiceProvider>(), environment);
            var guid = await usageService.GetUserGuid();
            Assert.IsTrue(guid.Equals(UserGuid));
        }

        [Test]
        public void IsSameDayWorks()
        {
            var usageService = new UsageService(Substitute.For<IGitHubServiceProvider>(), environment);
            var now = DateTimeOffset.Now;
            Assert.True(usageService.IsSameDay(now));
            Assert.True(usageService.IsSameDay(now));
            Assert.False(usageService.IsSameDay(now.AddDays(1)));
            Assert.False(usageService.IsSameDay(now.AddDays(-1)));
            Assert.True(usageService.IsSameDay(now.AddHours(10).AddMinutes(30).AddSeconds(1)));
            Assert.False(usageService.IsSameDay(now.AddDays(1).AddHours(10).AddMinutes(30).AddSeconds(1)));
            Assert.False(usageService.IsSameDay(now.AddDays(-1).AddHours(10).AddMinutes(30).AddSeconds(1)));
        }

        [Test]
        public void IsSameWeekWorks()
        {
            var usageService = new UsageService(Substitute.For<IGitHubServiceProvider>(), environment);
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
            var usageService = new UsageService(Substitute.For<IGitHubServiceProvider>(), environment);
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
