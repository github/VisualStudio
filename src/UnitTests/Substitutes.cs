using GitHub.Models;
using GitHub.Services;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using NSubstitute;
using Rothko;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    internal static class Substitutes
    {
        public static IGitRepositoriesExt IGitRepositoriesExt
        {
            get
            {
                var ret = Substitute.For<IGitRepositoriesExt>();
                return ret;
            }
        }

        public static IVSServices IVSServices
        {
            get
            {
                var ret = Substitute.For<IVSServices>();
                ret.GetLocalClonePathFromGitProvider(Args.ServiceProvider).Returns(@"c:\foo\bar");
                return ret;
            }
        }

        public static IOperatingSystem OperatingSystem
        {
            get
            {
                var ret = Substitute.For<IOperatingSystem>();
                return ret;
            }
        }

        public static IExportFactoryProvider ExportFactoryProvider { get { return Substitute.For<IExportFactoryProvider>(); } }

        public static IRepositoryCreationService RepositoryCreationService { get { return Substitute.For<IRepositoryCreationService>(); } }
        public static IRepositoryCloneService RepositoryCloneService { get { return Substitute.For<IRepositoryCloneService>(); } }

        public static IRepositoryHosts RepositoryHosts { get { return Substitute.For<IRepositoryHosts>(); } }

        /// <summary>
        /// This returns a service provider with everything mocked except for 
        /// RepositoryCloneService and RepositoryCreationService, which are real
        /// instances.
        /// </summary>
        public static IServiceProvider ServiceProvider { get { return GetServiceProvider();  } }

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
        public static IServiceProvider GetServiceProvider(
            IRepositoryCloneService cloneService = null,
            IRepositoryCreationService creationService = null)
        {
            var ret = Substitute.For<IServiceProvider, IUIProvider>();
            var os = OperatingSystem;
            var git = IGitRepositoriesExt;
            var vs = IVSServices;
            var clone = cloneService;
            if (clone == null)
                clone = new RepositoryCloneService(new Lazy<IServiceProvider>(() => ret), os);
            var create = creationService;
            if (create == null)
                create = new RepositoryCreationService(clone);
            var hosts = RepositoryHosts;
            var exports = ExportFactoryProvider;
            ret.GetService(typeof(IGitRepositoriesExt)).Returns(git);
            ret.GetService(typeof(IVSServices)).Returns(vs);
            ret.GetService(typeof(IOperatingSystem)).Returns(os);
            ret.GetService(typeof(IRepositoryCloneService)).Returns(clone);
            ret.GetService(typeof(IRepositoryCreationService)).Returns(create);
            ret.GetService(typeof(IRepositoryHosts)).Returns(hosts);
            ret.GetService(typeof(IExportFactoryProvider)).Returns(exports);
            return ret;
        }

        public static IGitRepositoriesExt GetGitExt(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IGitRepositoriesExt)) as IGitRepositoriesExt;
        }

        public static IVSServices GetVSServices(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IVSServices)) as IVSServices;
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

        public static IRepositoryHosts GetRepositoryHosts(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IRepositoryHosts)) as IRepositoryHosts;
        }

        public static IExportFactoryProvider GetExportFactoryProvider(this IServiceProvider provider)
        {
            return provider.GetService(typeof(IExportFactoryProvider)) as IExportFactoryProvider;
        }
    }
}
