using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Services;
using GitHub.VisualStudio.UI;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    [TeamExplorerSection(PlaceholderGitHubSectionId, TeamExplorerPageIds.Connect, 10)]
    public class PlaceholderGitHubSection : TeamExplorerSectionBase
    {
        public const string PlaceholderGitHubSectionId = "519B47D3-F2A9-4E19-8491-8C9FA25ABE97";

        protected GitHubConnectContent View
        {
            get { return this.SectionContent as GitHubConnectContent; }
            set { this.SectionContent = value; }
        }

        [ImportingConstructor]
        public PlaceholderGitHubSection([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {

            this.Title = "GitHub";
            this.IsVisible = true;
            this.IsEnabled = true;
            this.IsExpanded = true;
            View = new GitHubConnectContent();
            View.ViewModel = this;
        }

        public void DoCreate()
        {
            // this is done here and not via the constructor so nothing gets loaded
            // until we get here
            var ui = ServiceProvider.GetExportedValue<IUIProvider>();

            var factory = ui.GetService<ExportFactoryProvider>();
            var d = factory.UIControllerFactory.CreateExport();
            var creation = d.Value.SelectFlow(UIControllerFlow.Create);
            var x = new WindowController(creation);
            creation.Subscribe(_ => { }, _ => x.Close());
            x.Show();
        }

        public void DoClone()
        {
            // this is done here and not via the constructor so nothing gets loaded
            // until we get here
            var ui = ServiceProvider.GetExportedValue<IUIProvider>();

            var factory = ui.GetService<ExportFactoryProvider>();
            var d = factory.UIControllerFactory.CreateExport();
            var creation = d.Value.SelectFlow(UIControllerFlow.Clone);
            creation.Subscribe(_ => { }, _ => d.Dispose());
            var x = new WindowController(creation);
            x.Show();

            d.Value.Start();
        }
    }
}
