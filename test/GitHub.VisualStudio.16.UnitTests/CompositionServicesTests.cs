using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using NUnit.Framework;

public class CompositionServicesTests
{
    public class TheGetMinimalExportProviderMethod
    {
        [Test]
        public void Exports_IGitService()
        {
            var compositionContainer = CreateDefaultCompositionContainer();
            var target = new CompositionServices(compositionContainer);

            var exportProvider = target.GetMinimalExportProvider();

            var gitService = exportProvider.GetExportedValue<IGitService>();
            Assert.NotNull(gitService);
        }

        [Test]
        public void Exports_ISimpleApiClientFactory()
        {
            var compositionContainer = CreateDefaultCompositionContainer();
            var target = new CompositionServices(compositionContainer);

            var exportProvider = target.GetMinimalExportProvider();

            var simpleApiClientFactory = exportProvider.GetExportedValue<ISimpleApiClientFactory>();
            Assert.NotNull(simpleApiClientFactory);
        }

        [Test]
        public void Exports_IVSGitExt()
        {
            var compositionContainer = CreateDefaultCompositionContainer();
            var target = new CompositionServices(compositionContainer);

            var exportProvider = target.GetMinimalExportProvider();

            var gitExt = exportProvider.GetExportedValue<IVSGitExt>();
            Assert.NotNull(gitExt);
        }

        [Test]
        public void Exports_IUsageTracker()
        {
            var compositionContainer = CreateDefaultCompositionContainer();
            var target = new CompositionServices(compositionContainer);

            var exportProvider = target.GetMinimalExportProvider();

            var usageTracker = exportProvider.GetExportedValue<IUsageTracker>();
            Assert.NotNull(usageTracker);
        }

        [Test]
        public void Exports_IGitClient()
        {
            var compositionContainer = CreateDefaultCompositionContainer();
            var target = new CompositionServices(compositionContainer);

            var exportProvider = target.GetMinimalExportProvider();

            var gitClient = exportProvider.GetExportedValue<IGitClient>();
            Assert.NotNull(gitClient);
        }

        [Test]
        public void Exports_ITeamExplorerContext()
        {
            var compositionContainer = CreateDefaultCompositionContainer();
            var target = new CompositionServices(compositionContainer);

            var exportProvider = target.GetMinimalExportProvider();

            var gitExt = exportProvider.GetExportedValue<ITeamExplorerContext>();
            Assert.NotNull(gitExt);
        }

        static CompositionContainer CreateDefaultCompositionContainer()
        {
            var compositionContainer = new CompositionContainer();
            compositionContainer.ComposeExportedValue(CreateJoinableTaskContext());

            var contextFactory = Substitute.For<IVSUIContextFactory>();
            compositionContainer.ComposeExportedValue(contextFactory);

            var serviceProvider = Substitute.For<SVsServiceProvider>();
            compositionContainer.ComposeExportedValue(serviceProvider);

            return compositionContainer;
        }

        static JoinableTaskContext CreateJoinableTaskContext()
        {
#pragma warning disable VSSDK005 // Avoid instantiating JoinableTaskContext
            return new JoinableTaskContext();
#pragma warning restore VSSDK005 // Avoid instantiating JoinableTaskContext
        }
    }
}
