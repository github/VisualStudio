using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
using static System.FormattableString;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model which displays a pull request list.
    /// </summary>
    [Export(typeof(IPullRequestListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : IssueListViewModelBase, IPullRequestListViewModel
    {
        static readonly IReadOnlyList<string> states = new[] { "Open", "Closed", "All" };
        readonly IPullRequestSessionManager sessionManager;
        readonly IPullRequestService service;
        readonly IDisposable subscription;
        ObservableAsPropertyHelper<Uri> webUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestListViewModel"/> class.
        /// </summary>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="repositoryService">The repository service.</param>
        /// <param name="service">The pull request service.</param>
        [ImportingConstructor]
        public PullRequestListViewModel(
            IPullRequestSessionManager sessionManager,
            IRepositoryService repositoryService,
            IPullRequestService service)
            : base(repositoryService)
        {
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(service, nameof(service));

            this.sessionManager = sessionManager;
            this.service = service;

            subscription = sessionManager.WhenAnyValue(x => x.CurrentSession.PullRequest.Number).Subscribe(UpdateCurrent);
            webUrl = this.WhenAnyValue(x => x.RemoteRepository)
                .Select(x => x?.CloneUrl?.ToRepositoryUrl().Append("pulls"))
                .ToProperty(this, x => x.WebUrl);
            CreatePullRequest = ReactiveCommand.Create(() => NavigateTo("pull/new"));
            OpenItemInBrowser = ReactiveCommand.Create<IPullRequestListItemViewModel, IPullRequestListItemViewModel>(x => x);
        }

        /// <inheritdoc/>
        public override IReadOnlyList<string> States => states;

        /// <inheritdoc/>
        public Uri WebUrl => webUrl.Value;

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> CreatePullRequest { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestListItemViewModel, IPullRequestListItemViewModel> OpenItemInBrowser { get; }

        /// <inheritdoc/>
        protected override IVirtualizingListSource<IIssueListItemViewModelBase> CreateItemSource()
        {
            return new ItemSource(this);
        }

        /// <inheritdoc/>
        protected override Task DoOpenItem(IIssueListItemViewModelBase item)
        {
            var i = (IPullRequestListItemViewModel)item;
            NavigateTo(Invariant($"{RemoteRepository.Owner}/{RemoteRepository.Name}/pull/{i.Number}"));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task<Page<ActorModel>> LoadAuthors(string after)
        {
            return service.ReadAssignableUsers(
                HostAddress.Create(LocalRepository.CloneUrl),
                LocalRepository.Owner,
                LocalRepository.Name,
                after);
        }

        void UpdateCurrent(int number)
        {
            if (Items != null)
            {
                foreach (var i in Items)
                {
                    var item = i as PullRequestListItemViewModel;

                    if (item != null)
                    {
                        item.IsCurrent = item.Number == number;
                    }
                }
            }
        }

        class ItemSource : SequentialListSource<PullRequestListItemModel, IIssueListItemViewModelBase>
        {
            readonly PullRequestListViewModel owner;

            public ItemSource(PullRequestListViewModel owner)
            {
                this.owner = owner;
            }

            protected override IIssueListItemViewModelBase CreateViewModel(PullRequestListItemModel model)
            {
                var result = new PullRequestListItemViewModel(model);
                result.IsCurrent = owner.sessionManager.CurrentSession?.PullRequest.Number == model.Number;
                return result;
            }

            protected override async Task<Page<PullRequestListItemModel>> LoadPage(string after)
            {
                PullRequestStateEnum[] states;

                switch (owner.SelectedState)
                {
                    case "Open":
                        states = new[] { PullRequestStateEnum.Open };
                        break;
                    case "Closed":
                        states = new[] { PullRequestStateEnum.Closed, PullRequestStateEnum.Merged };
                        break;
                    default:
                        states = new[] { PullRequestStateEnum.Open, PullRequestStateEnum.Closed, PullRequestStateEnum.Merged };
                        break;
                }

                var result = await owner.service.ReadPullRequests(
                    HostAddress.Create(owner.RemoteRepository.CloneUrl),
                    owner.RemoteRepository.Owner,
                    owner.RemoteRepository.Name,
                    after,
                    states).ConfigureAwait(false);
                return result;
            }
        }
    }
}
