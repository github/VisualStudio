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
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Settings;

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
        public static IPackageSettings PackageSettings { get { return Substitute.For<IPackageSettings>(); } }
        public static IUsageTracker UsageTracker { get { return Substitute.For<IUsageTracker>(); } }
        public static IUsageService UsageService { get { return Substitute.For<IUsageService>(); } }
        public static IVSServices VSServices { get { return Substitute.For<IVSServices>(); } }

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
            cloneService = cloneService ?? new RepositoryCloneService(os, vsgit, Substitute.For<IUsageTracker>());
            creationService = creationService ?? new RepositoryCreationService(cloneService);
            avatarProvider = avatarProvider ?? Substitute.For<IAvatarProvider>();
            //ret.GetService(typeof(IGitRepositoriesExt)).Returns(IGitRepositoriesExt);
            ret.SetupMEF(gitservice);
            ret.SetupMEF(Substitute.For<IVSServices>());
            ret.SetupMEF(vsgit);
            ret.SetupMEF(os);
            ret.SetupMEF(cloneService);
            ret.SetupMEF(creationService);
            ret.SetupMEF(ViewViewModelFactory);
            ret.SetupMEF(Connection);
            ret.SetupMEF(ConnectionManager);
            ret.SetupMEF(avatarProvider);
            ret.SetupMEF(TwoFactorChallengeHandler);
            ret.SetupMEF(GistPublishService);
            ret.SetupMEF(PullRequestService);
            ret.SetupMEF(VSServices);

            ret.SetupService(PackageSettings);
            ret.SetupService(UsageTracker);
            ret.SetupService(UsageService);
            return ret;
        }

        public static void SetupMEF<T>(this IGitHubServiceProvider provider, T instance)
            where T : class
        {
            provider.TryGetMEFComponent<T>().Returns(instance);
            provider.TryGetMEFComponent(typeof(T)).Returns(instance);
            provider.GetMEFComponent<T>().Returns(instance);
        }

        public static void SetupService<T>(this IGitHubServiceProvider provider, T instance)
            where T : class
        {
            provider.TryGetServiceAsync<T>().Returns(Task.FromResult(instance));
            provider.TryGetServiceSync<T>().Returns(instance);
            provider.GetService(typeof(T)).Returns(instance);
        }

        //public static IGitRepositoriesExt GetGitExt(this IServiceProvider provider)
        //{
        //    return provider.GetService(typeof(IGitRepositoriesExt)) as IGitRepositoriesExt;
        //}

        public static IVSServices GetVSServices(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IVSServices)) as IVSServices;
        }

        public static IVSGitServices GetVSGitServices(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IVSGitServices)) as IVSGitServices;
        }

        public static IGitService GetGitService(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IGitService)) as IGitService;
        }

        public static IOperatingSystem GetOperatingSystem(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IOperatingSystem)) as IOperatingSystem;
        }

        public static IRepositoryCloneService GetRepositoryCloneService(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IRepositoryCloneService)) as IRepositoryCloneService;
        }

        public static IRepositoryCreationService GetRepositoryCreationService(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IRepositoryCreationService)) as IRepositoryCreationService;
        }

        public static IViewViewModelFactory GetExportFactoryProvider(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IViewViewModelFactory)) as IViewViewModelFactory;
        }

        public static IConnection GetConnection(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IConnection)) as IConnection;
        }

        public static IConnectionManager GetConnectionManager(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IConnectionManager)) as IConnectionManager;
        }

        public static IAvatarProvider GetAvatarProvider(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IAvatarProvider)) as IAvatarProvider;
        }

        public static IDelegatingTwoFactorChallengeHandler GetTwoFactorChallengeHandler(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IDelegatingTwoFactorChallengeHandler)) as IDelegatingTwoFactorChallengeHandler;
        }

        public static IGistPublishService GetGistPublishService(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IGistPublishService)) as IGistPublishService;
        }

        public static IPullRequestService GetPullRequestsService(this IGitHubServiceProvider provider)
        {
            return provider.TryGetMEFComponent(typeof(IPullRequestService)) as IPullRequestService;
        }
    }
}
