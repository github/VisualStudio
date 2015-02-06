using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using LibGit2Sharp;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(WikiNavigationItemId,
        NavigationItemPriority.Wiki,
        TargetPageId = TeamExplorerPageIds.Home)]
    class WikiNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string WikiNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA1";

        [ImportingConstructor]
        public WikiNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            Octokit.IGitHubClient client)
            : base(serviceProvider)
        {
            Text = "Wiki";


            IsVisible = false;
            IsEnabled = true;

            Image = Resources.book;

        }

        async void UpdateState()
        {
            var solution = ServiceProvider.GetSolution();
            IsEnabled = await solution.IsHostedOnGitHub();
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            /*
            ITeamFoundationContext context = e.NewContext;
            if (context != null && context.HasCollection && context.HasTeamProject)
            {
                VersionControlServer vcs = context.TeamProjectCollection.GetService<VersionControlServer>();
                if (vcs != null)
                {
                    
                }
            }
            */

            base.ContextChanged(sender, e);
        }

		public override void Execute()
		{
			base.Execute();
		}
	}
}
