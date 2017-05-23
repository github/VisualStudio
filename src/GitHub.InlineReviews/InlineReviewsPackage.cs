using System;
using System.Runtime.InteropServices;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Views;
using GitHub.Models;
using GitHub.Primitives;
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
        public async void ShowPullRequestComments(IPullRequestModel pullRequest)
        {
            var window = (PullRequestCommentsPane)FindToolWindow(
                typeof(PullRequestCommentsPane), pullRequest.Number, true);

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create Pull Request Comments tool window");
            }

            var serviceProvider = (IGitHubServiceProvider)GetGlobalService(typeof(IGitHubServiceProvider));
            var manager = serviceProvider.GetService<IPullRequestSessionManager>();
            var session = await manager.GetSession(pullRequest);
            var address = HostAddress.Create(session.Repository.CloneUrl);
            var apiClientFactory = serviceProvider.GetService<IApiClientFactory>();
            var apiClient = apiClientFactory.Create(address);
            await window.Initialize(session, apiClient);

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.AddCommandHandler<IPullRequestModel>(
                GlobalCommands.CommandSetGuid,
                GlobalCommands.ShowPullRequestCommentsId,
                () => true,
                ShowPullRequestComments);
        }
    }
}
