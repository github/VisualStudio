using System;
using System.ComponentModel;
using System.Threading;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using NSubstitute;
using Rothko;
using Xunit;

namespace UnitTests.GitHub.TeamFoundation._14
{
    public class TeamExplorerServiceHolderTests
    {
        [Fact]
        public void SettingServiceProviderShouldSignalSubscriber()
        {
            var services = CreateServices();
            var target = new TeamExplorerServiceHolder(services.Factory, services.Os);
            var who = new object();
            var evt = new ManualResetEvent(false);
            var handler = (Action<ILocalRepositoryModel>)(x => evt.Set());

            target.Subscribe(who, handler);
            target.ServiceProvider = services.ServiceProvider;

            Assert.True(evt.WaitOne(100));
        }

        [Fact]
        public void GitServicePropertyChangedShouldSignalSubscriber()
        {
            var services = CreateServices();
            var target = new TeamExplorerServiceHolder(services.Factory, services.Os);
            var who = new object();
            var evt = new ManualResetEvent(false);
            var handler = (Action<ILocalRepositoryModel>)(x => evt.Set());

            target.Subscribe(who, handler);
            target.ServiceProvider = services.ServiceProvider;
            Assert.True(evt.WaitOne(100));

            evt.Reset();
            services.GitExt.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(
                services.GitExt,
                new PropertyChangedEventArgs(nameof(services.GitExt.ActiveRepositories)));
            Assert.True(evt.WaitOne(100));
        }

        Services CreateServices()
        {
            var factory = Substitute.For<IUIContextFactory>();
            var uiContext = Substitute.For<IUIContextWrapper>();
            uiContext.IsActive.Returns(true);
            factory.FromUIContextGuid(Arg.Any<Guid>()).Returns(uiContext);

            var serviceProvider = Substitutes.ServiceProvider;
            var uiProvider = (IUIProvider)serviceProvider;
            var gitExt = Substitute.For<IGitExt>();
            var repositoryInfo = Substitute.For<IGitRepositoryInfo>();
            gitExt.ActiveRepositories.Returns(new[] { repositoryInfo });
            uiProvider.TryGetService(typeof(IGitExt)).Returns(gitExt);

            var os = Substitute.For<IOperatingSystem>();
            var dirInfo = Substitute.For<IDirectoryInfo>();
            dirInfo.Exists.Returns(true);
            os.Directory.GetDirectory(Arg.Any<string>()).Returns(dirInfo);

            return new Services(factory, os, serviceProvider, gitExt);
        }

        class Services
        {
            public Services(
                IUIContextFactory factory,
                IOperatingSystem os,
                IServiceProvider serviceProvider,
                IGitExt gitExt)
            {
                Factory = factory;
                Os = os;
                ServiceProvider = serviceProvider;
                GitExt = gitExt;
            }

            public IUIContextFactory Factory { get; }
            public IOperatingSystem Os { get; }
            public IServiceProvider ServiceProvider { get; }
            public IGitExt GitExt { get; }
        }
    }
}
