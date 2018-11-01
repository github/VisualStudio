using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    [Export(typeof(IIssueishPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueishPaneViewModel : ViewModelBase, IIssueishPaneViewModel
    {
        readonly IViewViewModelFactory factory;
        readonly IPullRequestSessionManager sessionManager;
        IViewModel content;
        string paneCaption;

        [ImportingConstructor]
        public IssueishPaneViewModel(
            IViewViewModelFactory factory,
            IPullRequestSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.factory = factory;
            this.sessionManager = sessionManager;
        }

        public IViewModel Content
        {
            get => content;
            private set => this.RaiseAndSetIfChanged(ref content, value);
        }

        public string PaneCaption
        {
            get => paneCaption;
            private set => this.RaiseAndSetIfChanged(ref paneCaption, value);
        }

        public Task InitializeAsync(IServiceProvider paneServiceProvider)
        {
            return Task.CompletedTask;
        }

        public async Task Load(IConnection connection, string owner, string name, int number)
        {
            Content = new SpinnerViewModel();
            PaneCaption = "#" + number;

            // TODO: We will eventually support loading issues here as well.
            try
            {
                var session = await sessionManager.GetSession(owner, name, number).ConfigureAwait(true);
                var vm = factory.CreateViewModel<IPullRequestPageViewModel>();

                var repository = new RemoteRepositoryModel(
                    0,
                    name,
                    session.LocalRepository.CloneUrl.WithOwner(session.PullRequest.HeadRepositoryOwner),
                    false,
                    false,
                    null,
                    null);

                await vm.InitializeAsync(
                    repository,
                    session.LocalRepository,
                    session.User,
                    session.PullRequest).ConfigureAwait(true);
                Content = vm;
                PaneCaption += " " + vm.Title;
            }
            catch (Exception ex)
            {
                // TODO: Show exception.
            }
        }
    }
}
