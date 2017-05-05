using System;
using System.Runtime.InteropServices;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Commands;
using GitHub.InlineReviews.Views;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.InlineReviewsPackageId)]
    [ProvideAutoLoad(UIContextGuids80.CodeWindow)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(PullRequestCommentsPane), DocumentLikeTool=true)]
    public class InlineReviewsPackage : Package
    {
        public void ShowPullRequestComments()
        {
            var window = (PullRequestCommentsPane)FindToolWindow(
                typeof(PullRequestCommentsPane), 0, true);

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create Pull Request Comments tool window");
            }

            var serviceProvider = (IGitHubServiceProvider)GetGlobalService(typeof(IGitHubServiceProvider));
            var manager = serviceProvider.GetService<IPullRequestReviewSessionManager>();
            var apiClientFactory = serviceProvider.GetService<IApiClientFactory>();
            window.Initialize(manager, apiClientFactory);

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.AddCommandHandler(
                ShowPullRequestCommentsCommand.CommandSet,
                ShowPullRequestCommentsCommand.CommandId,
                (s, e) => ShowPullRequestComments());
            //ShowPullRequestCommentsCommand.Initialize(this);
        }
    }
}
