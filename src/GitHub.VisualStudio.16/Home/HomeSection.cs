using System;
using System.ComponentModel.Composition;
using GitHub.VisualStudio;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;

namespace Microsoft.TeamExplorerSample.Sync
{
    /// <summary>
    /// Publish section.
    /// </summary>
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.Home, Priority)]
    public class HomeSection : TeamExplorerBaseSection
    {
        readonly CompositionServices compositionServices;

        public const string SectionId = "C655016C-CDCF-4E04-8B8F-DD769B740A8A";
        public const int Priority = 10;

        /// <summary>
        /// Constructor.
        /// </summary>
        [ImportingConstructor]
        public HomeSection(CompositionServices compositionServices)
            : base()
        {
            this.compositionServices = compositionServices;

            Title = "GitHub";
            this.IsExpanded = true;
            this.IsBusy = false;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            var isInstalled = FullExtensionUtilities.IsInstalled(ServiceProvider);

            if (isInstalled)
            {
                IsVisible = false;
                return;
            }

            var exportProvider = this.compositionServices.GetExportProvider();
            var homeSectionViewModel = exportProvider.GetExportedValue<HomeSectionViewModel>();

            View = new HomeSectionView
            {
                DataContext = homeSectionViewModel
            };
        }

        protected HomeSectionView View
        {
            get { return SectionContent as HomeSectionView; }
            set { SectionContent = value; }
        }
    }
}
