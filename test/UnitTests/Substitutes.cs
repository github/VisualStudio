using GitHub.App.Factories;
using GitHub.Authentication;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using NSubstitute;
using Rothko;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using GitHub.Api;

namespace UnitTests
{
    internal static class Substitutes
    {
        public static T1 For<T1, T2, T3, T4>(params object[] constructorArguments)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return (T1)Substitute.For(new Type[4]
            {
                typeof (T1),
                typeof (T2),
                typeof (T3),
                typeof (T4)
            }, constructorArguments);
        }


       // public static IGitRepositoriesExt IGitRepositoriesExt { get { return Substitute.For<IGitRepositoriesExt>(); } }
        public static IGitService IGitService { get { return Substitute.For<IGitService>(); } }

        public static IVSGitServices IVSGitServices
        {
            get
            {
                var ret = Substitute.For<IVSGitServices>();
                ret.GetLocalClonePathFromGitProvider().Returns(@"c:\foo\bar");
                return ret;
            }
        }

        public static IOperatingSystem OperatingSystem
        {
            get
            {
                var ret = Substitute.For<IOperatingSystem>();
                // this expansion happens when the GetLocalClonePathFromGitProvider call is setup by default
                // see IVSServices property above
                ret.Environment.ExpandEnvironmentVariables(Args.String).Returns(x => x[0]);
                return ret;
            }
        }

        public static IExportFactoryProvider ExportFactoryProvider { get { return Substitute.For<IExportFactoryProvider>(); } }
        public static IUIFactory UIFactory { get { return Substitute.For<IUIFactory>(); } }

        public static IRepositoryCreationService RepositoryCreationService { get { return Substitute.For<IRepositoryCreationService>(); } }
        public static IRepositoryCloneService RepositoryCloneService { get { return Substitute.For<IRepositoryCloneService>(); } }

        public static IConnection Connection { get { return Substitute.For<IConnection>(); } }
        public static IConnection NewConnection { get { return Substitute.For<IConnection>(); } }
        public static IConnectionManager ConnectionManager { get { return Substitute.For<IConnectionManager>(); } }
        public static IConnectionManager NewConnectionManager { get { return Substitute.For<IConnectionManager>(); } }
        public static IDelegatingTwoFactorChallengeHandler TwoFactorChallengeHandler { get { return Substitute.For<IDelegatingTwoFactorChallengeHandler>(); } }
        public static IGistPublishService GistPublishService { get { return Substitute.For<IGistPublishService>(); } }
        public static IPullRequestService PullRequestService { get { return Substitute.For<IPullRequestService>(); } }
        public static IUIProvider UIProvider { get { return Substitute.For<IUIProvider>(); } }

        /// <summary>
        /// This returns a service provider with everything mocked except for 
        /// RepositoryCloneService and RepositoryCreationService, which are real
        /// instances.
        /// </summary>
        public static IGitHubServiceProvider ServiceProvider { get { return GetServiceProvider();  } }

        /// <summary>
        /// This returns a service provider with mocked IRepositoryCreationService and
        /// IRepositoryCloneService as well as all other services mocked. The regular
        /// GetServiceProvider method (and ServiceProvider property return a IServiceProvider
        /// with real RepositoryCloneService and RepositoryCreationService instances.
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider GetFullyMockedServiceProvider()
        {
            return GetServiceProvider(RepositoryCloneService, RepositoryCreationService);
        }

        /// <summary>
        /// This returns a service provider with everything mocked except for 
        /// RepositoryCloneService and RepositoryCreationService, which are real
        /// instances.
        /// </summary>
        /// <param name="cloneService"></param>
        /// <param name="creationService"></param>
        /// <returns></returns>
        public static IGitHubServiceProvider GetServiceProvider(
            IRepositoryCloneService cloneService = null,
            IRepositoryCreationService creationService = null,
            IAvatarProvider avatarProvider = null)
        {
            var ret = Substitute.For<IGitHubServiceProvider, IServiceProvider>();

            var gitservice = IGitService;
            var cm = Substitute.For<SComponentModel, IComponentModel>();
            var cc = new CompositionContainer(CompositionOptions.IsThreadSafe | CompositionOptions.DisableSilentRejection);
            cc.ComposeExportedValue(gitservice);
            ((IComponentModel)cm).DefaultExportProvider.Returns(cc);
            ret.GetService(typeof(SComponentModel)).Returns(cm);
            Services.UnitTestServiceProvider = ret;

            var os = OperatingSystem;
            var vsgit = IVSGitServices;
            var clone = cloneService ?? new RepositoryCloneService(os, vsgit, Substitute.For<IUsageTracker>());
            var create = creationService ?? new RepositoryCreationService(clone);
            avatarProvider = avatarProvider ?? Substitute.For<IAvatarProvider>();
            //ret.GetService(typeof(IGitRepositoriesExt)).Returns(IGitRepositoriesExt);
            ret.GetService(typeof(IGitService)).Returns(gitservice);
            ret.GetService(typeof(IVSServices)).Returns(Substitute.For<IVSServices>());
            ret.GetService(typeof(IVSGitServices)).Returns(vsgit);
            ret.GetService(typeof(IOperatingSystem)).Returns(os);
            ret.GetService(typeof(IRepositoryCloneService)).Returns(clone);
            ret.GetService(typeof(IRepositoryCreationService)).Returns(create);
            ret.GetService(typeof(IExportFactoryProvider)).Returns(ExportFactoryProvider);
            ret.GetService(typeof(IUIFactory)).Returns(UIFactory);
            ret.GetService(typeof(IConnection)).Returns(Connection);
            ret.GetService(typeof(IConnection)).Returns(NewConnection);
            ret.GetService(typeof(IConnectionManager)).Returns(ConnectionManager);
            ret.GetService(typeof(IConnectionManager)).Returns(NewConnectionManager);
            ret.GetService(typeof(IAvatarProvider)).Returns(avatarProvider);
            ret.GetService(typeof(IDelegatingTwoFactorChallengeHandler)).Returns(TwoFactorChallengeHandler);
            ret.GetService(typeof(IGistPublishService)).Returns(GistPublishService);
            ret.GetService(typeof(IPullRequestService)).Returns(PullRequestService);
            ret.GetService(typeof(IUIProvider)).Returns(UIProvider);
            return ret;
        }

        //public static IGitRepositoriesExt GetGitExt(this IServiceProvider provider)
        //{
        //    return provider.GetService(typeof(IGitRepositoriesExt)) as IGitRepositoriesExt;
        //}

        public static IVSServices GetVSServices(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IVSServices)) as IVSServices;
        }

        public static IVSGitServices GetVSGitServices(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IVSGitServices)) as IVSGitServices;
        }

        public static IGitService GetGitService(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IGitService)) as IGitService;
        }

        public static IOperatingSystem GetOperatingSystem(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IOperatingSystem)) as IOperatingSystem;
        }

        public static IRepositoryCloneService GetRepositoryCloneService(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IRepositoryCloneService)) as IRepositoryCloneService;
        }

        public static IRepositoryCreationService GetRepositoryCreationService(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IRepositoryCreationService)) as IRepositoryCreationService;
        }

        public static IExportFactoryProvider GetExportFactoryProvider(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IExportFactoryProvider)) as IExportFactoryProvider;
        }
        public static IUIFactory GetUIFactory(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IUIFactory)) as IUIFactory;
        }

        public static IConnection GetConnection(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IConnection)) as IConnection;
        }

        public static IConnection GetNewConnection(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IConnection)) as IConnection;
        }

        public static IConnectionManager GetConnectionManager(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IConnectionManager)) as IConnectionManager;
        }

        public static IConnectionManager GetNewConnectionManager(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IConnectionManager)) as IConnectionManager;
        }

        public static IAvatarProvider GetAvatarProvider(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IAvatarProvider)) as IAvatarProvider;
        }

        public static IDelegatingTwoFactorChallengeHandler GetTwoFactorChallengeHandler(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IDelegatingTwoFactorChallengeHandler)) as IDelegatingTwoFactorChallengeHandler;
        }

        public static IGistPublishService GetGistPublishService(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IGistPublishService)) as IGistPublishService;
        }

        public static IPullRequestService GetPullRequestsService(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IPullRequestService)) as IPullRequestService;
        }

        public static IUIProvider GetUIProvider(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IUIProvider)) as IUIProvider;
        }
    }
}
