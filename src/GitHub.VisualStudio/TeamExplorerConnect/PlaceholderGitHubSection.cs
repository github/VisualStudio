using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using GitHub.Models;

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    [TeamExplorerSection(PlaceholderGitHubSectionId, TeamExplorerPageIds.Connect, 10)]
    public class PlaceholderGitHubSection : TeamExplorerSectionBase
    {
        public const string PlaceholderGitHubSectionId = "519B47D3-F2A9-4E19-8491-8C9FA25ABE97";

        IServiceProvider gitServiceProvider;

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

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            gitServiceProvider = e.ServiceProvider;
        }

        public void DoCreate()
        {
            // this is done here and not via the constructor so nothing gets loaded
            // until we get here
            StartFlow(UIControllerFlow.Create);
        }

        public void DoClone()
        {
            // this is done here and not via the constructor so nothing gets loaded
            // until we get here
            StartFlow(UIControllerFlow.Clone);
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.GitServiceProvider = gitServiceProvider;
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiControllerExport = factory.UIControllerFactory.CreateExport();
            var uiController = uiControllerExport.Value;
            var creation = uiController.SelectFlow(controllerFlow);
            
            var windowController = new WindowController(creation);
            creation.Subscribe(_ => { }, _ =>
            {
                windowController.Close();
                uiControllerExport.Dispose();
            });
            windowController.Show();

            uiController.Start();
        }
    }
}
