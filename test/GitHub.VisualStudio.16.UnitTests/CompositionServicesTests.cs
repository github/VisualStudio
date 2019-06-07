using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using NUnit.Framework;

public class CompositionServicesTests
{
    class TheGetMinimalExportProviderMethod
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
        public void Exports_IVSGitExt()
        {
            var compositionContainer = CreateDefaultCompositionContainer();
            var target = new CompositionServices(compositionContainer);

            var exportProvider = target.GetMinimalExportProvider();

            var gitExt = exportProvider.GetExportedValue<IVSGitExt>();
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
