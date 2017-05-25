using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.InlineReviews.Views;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.InlineReviews.Commands
{
    [Export(typeof(ICommand))]
    class ShowPullRequestCommentsCommand : Command<IPullRequestModel>
    {
        public static readonly Guid CommandSet = GlobalCommands.CommandSetGuid;
        public const int CommandId = GlobalCommands.ShowPullRequestCommentsId;

        [ImportingConstructor]
        public ShowPullRequestCommentsCommand()
            : base(CommandSet, CommandId)
        {
        }

        protected override async Task Execute(IPullRequestModel pullRequest)
        {
            var window = (PullRequestCommentsPane)Package.FindToolWindow(
                typeof(PullRequestCommentsPane), pullRequest.Number, true);

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create Pull Request Comments tool window");
            }

            var serviceProvider = GitHubServiceProvider;
            var manager = serviceProvider.GetService<IPullRequestSessionManager>();
            var session = await manager.GetSession(pullRequest);
            var address = HostAddress.Create(session.Repository.CloneUrl);
            var apiClientFactory = serviceProvider.GetService<IApiClientFactory>();
            var apiClient = apiClientFactory.Create(address);
            await window.Initialize(session, apiClient);

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
