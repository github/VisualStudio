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
using GitHub.Factories;
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

        public static IViewViewModelFactory ViewViewModelFactory { get { return Substitute.For<IViewViewModelFactory>(); } }

        public static IRepositoryCreationService RepositoryCreationService { get { return Substitute.For<IRepositoryCreationService>(); } }
        public static IRepositoryCloneService RepositoryCloneService { get { return Substitute.For<IRepositoryCloneService>(); } }

        public static IConnection Connection { get { return Substitute.For<IConnection>(); } }
        public static IConnectionManager ConnectionManager { get { return Substitute.For<IConnectionManager>(); } }
        public static IDelegatingTwoFactorChallengeHandler TwoFactorChallengeHandler { get { return Substitute.For<IDelegatingTwoFactorChallengeHandler>(); } }
        public static IGistPublishService GistPublishService { get { return Substitute.For<IGistPublishService>(); } }
        public static IPullRequestService PullRequestService { get { return Substitute.For<IPullRequestService>(); } }

        /// <summary>
        /// This returns a service provider with everything mocked except for 
        /// RepositoryCloneService and RepositoryCreationService, which are real
        /// instances.
        /// </summary>
        public static IGitHubServiceProvider ServiceProvider { get { return GetServiceProvider(); } }

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
            var clone = cloneService ?? new RepositoryCloneService(os, vsgit, Substitute.For<ITeamExplorerServices>(),
                Substitute.For<IGraphQLClientFactory>(), Substitute.For<IGitHubContextService>(),
                Substitute.For<IUsageTracker>(), ret);
            var create = creationService ?? new RepositoryCreationService(clone);
            avatarProvider = avatarProvider ?? Substitute.For<IAvatarProvider>();
            ret.GetService(typeof(IGitService)).Returns(gitservice);
            ret.GetService(typeof(IVSServices)).Returns(Substitute.For<IVSServices>());
            ret.GetService(typeof(ITeamExplorerServices)).Returns(Substitute.For<ITeamExplorerServices>());
            ret.GetService(typeof(IGraphQLClientFactory)).Returns(Substitute.For<IGraphQLClientFactory>());
            ret.GetService(typeof(IGitHubContextService)).Returns(Substitute.For<IGitHubContextService>());
            ret.GetService(typeof(IVSGitExt)).Returns(Substitute.For<IVSGitExt>());
            ret.GetService(typeof(IUsageTracker)).Returns(Substitute.For<IUsageTracker>());
            ret.GetService(typeof(IVSGitServices)).Returns(vsgit);
            ret.GetService(typeof(IOperatingSystem)).Returns(os);
            ret.GetService(typeof(IRepositoryCloneService)).Returns(clone);
            ret.GetService(typeof(IRepositoryCreationService)).Returns(create);
            ret.GetService(typeof(IViewViewModelFactory)).Returns(ViewViewModelFactory);
            ret.GetService(typeof(IConnection)).Returns(Connection);
            ret.GetService(typeof(IConnectionManager)).Returns(ConnectionManager);
            ret.GetService(typeof(IAvatarProvider)).Returns(avatarProvider);
            ret.GetService(typeof(IDelegatingTwoFactorChallengeHandler)).Returns(TwoFactorChallengeHandler);
            ret.GetService(typeof(IGistPublishService)).Returns(GistPublishService);
            ret.GetService(typeof(IPullRequestService)).Returns(PullRequestService);
            return ret;
        }

        public static IVSServices GetVSServices(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IVSServices)) as IVSServices;
        }

        public static ITeamExplorerServices GetTeamExplorerServices(this IServiceProvider provider)
        {
            return provider.GetService(typeof(ITeamExplorerServices)) as ITeamExplorerServices;
        }

        public static IGraphQLClientFactory GetGraphQLClientFactory(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IGraphQLClientFactory)) as IGraphQLClientFactory;
        }

        public static IGitHubContextService GetGitHubContextService(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IGitHubContextService)) as IGitHubContextService;
        }

        public static IVSGitExt GetVSGitExt(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IVSGitExt)) as IVSGitExt;
        }

        public static IUsageTracker GetUsageTracker(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IUsageTracker)) as IUsageTracker;
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

        public static IViewViewModelFactory GetExportFactoryProvider(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IViewViewModelFactory)) as IViewViewModelFactory;
        }

        public static IConnection GetConnection(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IConnection)) as IConnection;
        }

        public static IConnectionManager GetConnectionManager(this IServiceProvider provider)
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
    }
}
