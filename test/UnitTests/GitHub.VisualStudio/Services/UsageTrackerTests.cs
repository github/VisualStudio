using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using NSubstitute;
using Xunit;

namespace UnitTests.GitHub.VisualStudio.Services
{
    public class UsageTrackerTests
    {
        static readonly Guid UserGuid = Guid.NewGuid();

        public class TheTimer : TestBaseClass
        {
            [Fact]
            public void ShouldStartTimer()
            {
                var service = Substitute.For<IUsageService>();
                var target = new UsageTracker(CreateServiceProvider(), service);

                service.Received(1).StartTimer(Arg.Any<Func<Task>>(), TimeSpan.FromMinutes(3), TimeSpan.FromHours(8));
            }

            [Fact]
            public async Task FirstTickShouldIncrementLaunchCount()
            {
                var service = CreateUsageService();
                var targetAndTick = CreateTargetAndGetTick(CreateServiceProvider(), service);

                await targetAndTick.Item2();

                await service.Received(1).WriteLocalData(
                    Arg.Is<UsageData>(x =>
                        x.Reports.Count == 1 &&
                        x.Reports[0].Dimensions.Guid == UserGuid &&
                        x.Reports[0].Measures.NumberOfStartups == 1));
            }

            [Fact]
            public async Task FirstTickShouldIncrementLaunchCountInExistingReport()
            {
                var model = new UsageModel()
                {
                    Dimensions = new UsageModel.DimensionsModel()
                    {
                        Date = DateTimeOffset.Now,
                    },
                    Measures = new UsageModel.MeasuresModel
                    {
                        NumberOfStartups = 4,
                    }
                };
                var service = CreateUsageService(model);
                var targetAndTick = CreateTargetAndGetTick(CreateServiceProvider(), service);

                await targetAndTick.Item2();

                Assert.Equal(5, model.Measures.NumberOfStartups);
            }

            [Fact]
            public async Task FirstTickNotShouldIncrementLaunchCountInOldReport()
            {
                var model = new UsageModel
                {
                    Dimensions = new UsageModel.DimensionsModel()
                    {
                        Date = DateTimeOffset.Now - TimeSpan.FromDays(1),
                    },
                    Measures = new UsageModel.MeasuresModel
                    {
                        NumberOfStartups = 4,
                    }
                };
                var service = CreateUsageService(model);
                var targetAndTick = CreateTargetAndGetTick(CreateServiceProvider(), service);

                await targetAndTick.Item2();

                Assert.Equal(4, model.Measures.NumberOfStartups);
            }

            [Fact]
            public async Task SubsequentTickShouldNotIncrementLaunchCount()
            {
                var service = CreateUsageService();
                var targetAndTick = CreateTargetAndGetTick(CreateServiceProvider(), service);

                await targetAndTick.Item2();
                service.ClearReceivedCalls();
                await targetAndTick.Item2();

                await service.DidNotReceiveWithAnyArgs().WriteLocalData(null);
            }

            [Fact]
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

            [Fact]
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

            [Fact]
            public async Task TickShouldSendDataIfDifferentDay()
            {
                var model = new UsageModel
                {
                    Dimensions = new UsageModel.DimensionsModel()
                    {
                        Date = DateTimeOffset.Now - TimeSpan.FromDays(1)
                    },
                };
                var serviceProvider = CreateServiceProvider();
                var targetAndTick = CreateTargetAndGetTick(
                    serviceProvider,
                    CreateUsageService(model));

                await targetAndTick.Item2();

                var metricsService = serviceProvider.TryGetService<IMetricsService>();
                await metricsService.Received(1).PostUsage(Arg.Any<UsageModel>());
            }

            [Fact]
            public async Task TickShouldSendBacklogData()
            {
                var models = new[]
                {
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {
                            Date = DateTimeOffset.Now - TimeSpan.FromDays(2),
                            Guid = Guid.NewGuid()
                        }
                    },
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {
                            Date = DateTimeOffset.Now - TimeSpan.FromDays(1),
                            Guid = Guid.NewGuid()
                        }
                    },
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {
                            Date = DateTimeOffset.Now,
                            Guid = Guid.NewGuid()
                        }
                    },
                };
                var service = CreateUsageService(models);
                var serviceProvider = CreateServiceProvider();
                var targetAndTick = CreateTargetAndGetTick(serviceProvider, service);

                await targetAndTick.Item2();

                var metricsService = serviceProvider.TryGetService<IMetricsService>();

                await metricsService.Received(1).PostUsage(Arg.Is<UsageModel>(x => x.Dimensions.Guid == models[0].Dimensions.Guid));
                await metricsService.Received(1).PostUsage(Arg.Is<UsageModel>(x => x.Dimensions.Guid == models[1].Dimensions.Guid));
                await metricsService.Received(0).PostUsage(Arg.Is<UsageModel>(x => x.Dimensions.Guid == models[2].Dimensions.Guid));
            }

            [Fact]
            public async Task SentReportsShouldBeRemovedFromDisk()
            {
                var models = new[]
                {
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {Date = DateTimeOffset.Now - TimeSpan.FromDays(2), Guid = Guid.NewGuid()}
                    },
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {Date = DateTimeOffset.Now - TimeSpan.FromDays(1), Guid = Guid.NewGuid()}
                    },
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {Date = DateTimeOffset.Now, Guid = Guid.NewGuid()}
                    },
                };
                var service = CreateUsageService(models);
                var serviceProvider = CreateServiceProvider();
                var targetAndTick = CreateTargetAndGetTick(serviceProvider, service);

                await targetAndTick.Item2();

                await service.Received(1).WriteLocalData(Arg.Is<UsageData>(x => x.Reports.Count == 1));
            }

            [Fact]
            public async Task ReportsShouldNotBeRemovedIfNotSent()
            {
                var models = new[]
                {
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {Date = DateTimeOffset.Now - TimeSpan.FromDays(2), Guid = Guid.NewGuid()}
                    },
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {Date = DateTimeOffset.Now - TimeSpan.FromDays(1), Guid = Guid.NewGuid()}
                    },
                    new UsageModel
                    {
                        Dimensions = new UsageModel.DimensionsModel()
                        {Date = DateTimeOffset.Now, Guid = Guid.NewGuid()}
                    },
                };
                var service = CreateUsageService(models);
                var serviceProvider = CreateServiceProvider();
                var targetAndTick = CreateTargetAndGetTick(serviceProvider, service);
                var metricsService = serviceProvider.TryGetService<IMetricsService>();

                metricsService.PostUsage(null).ReturnsForAnyArgs(_ => { throw new Exception(); });

                await targetAndTick.Item2();

                await service.Received(1).WriteLocalData(Arg.Is<UsageData>(x => x.Reports.Count == 3));
            }
        }

        public class TheIncrementCounterMethod : TestBaseClass
        {
            [Fact]
            public async Task ShouldIncrementCounter()
            {
                var model = new UsageModel
                {
                    Dimensions = new UsageModel.DimensionsModel()
                    {
                        Date = DateTimeOffset.Now,
                    },
                    Measures = new UsageModel.MeasuresModel
                    {
                        NumberOfClones = 4
                    }
                };
                var target = new UsageTracker(
                    CreateServiceProvider(),
                    CreateUsageService(model));

                await target.IncrementCounter(x => x.NumberOfClones);

                Assert.Equal(5, model.Measures.NumberOfClones);
            }

            [Fact]
            public async Task ShouldWriteUpdatedData()
            {
                var data = new UsageModel
                {
                    Dimensions = new UsageModel.DimensionsModel()
                    { Date = DateTimeOffset.Now, },
                };
                var service = CreateUsageService(data);
                var target = new UsageTracker(
                    CreateServiceProvider(),
                    service);

                await target.IncrementCounter(x => x.NumberOfClones);

                await service.Received(1).WriteLocalData(
                    Arg.Is<UsageData>(x =>
                        x.Reports.Count == 1 &&
                        x.Reports[0].Measures.NumberOfClones == 1));
            }

            [Fact]
            public async Task ShouldCreateNewRecordIfNewDay()
            {
                var data = new UsageModel
                {
                    Dimensions = new UsageModel.DimensionsModel()
                    { Date = DateTimeOffset.Now - TimeSpan.FromDays(1),}
                };
                var service = CreateUsageService(data);
                var target = new UsageTracker(
                    CreateServiceProvider(),
                    service);

                await target.IncrementCounter(x => x.NumberOfClones);

                await service.Received(1).WriteLocalData(
                    Arg.Is<UsageData>(x => 
                        x.Reports.Count == 2 &&
                        x.Reports[1].Dimensions.Guid == UserGuid &&
                        x.Reports[1].Measures.NumberOfClones == 1));
            }
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

        static IUsageService CreateUsageService(params UsageModel[] reports)
        {
            foreach (var report in reports)
            {
                report.Dimensions = report.Dimensions ?? new UsageModel.DimensionsModel();
                report.Measures = report.Measures ?? new UsageModel.MeasuresModel();
            }

            return CreateUsageService(new UsageData { Reports = reports.ToList() });
        }

        static IUsageService CreateUsageService(UsageData data)
        {
            var result = Substitute.For<IUsageService>();
            result.GetUserGuid().Returns(UserGuid);
            result.ReadLocalData().Returns(data);
            return result;
        }
    }
}
