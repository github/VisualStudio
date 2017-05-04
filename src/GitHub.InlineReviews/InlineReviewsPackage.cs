using System;
using System.Runtime.InteropServices;
using GitHub.Extensions;
using GitHub.InlineReviews.Commands;
using GitHub.InlineReviews.Views;
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
            var window = FindToolWindow(typeof(PullRequestCommentsPane), 0, true);

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create Pull Request Comments tool window");
            }

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
