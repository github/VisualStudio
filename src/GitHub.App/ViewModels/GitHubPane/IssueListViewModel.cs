using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model which displays an issue list.
    /// </summary>
    [Export(typeof(IIssueListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueListViewModel : IssueListViewModelBase, IIssueListViewModel
    {
        static readonly IReadOnlyList<string> states = new[] { "Open", "Closed", "All" };
        readonly IIssueService service;
        readonly IShowIssueDetailsCommand showIssueDetails;
        ObservableAsPropertyHelper<Uri> webUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueListViewModel"/> class.
        /// </summary>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="repositoryService">The repository service.</param>
        /// <param name="service">The issue service.</param>
        [ImportingConstructor]
        public IssueListViewModel(
            IRepositoryService repositoryService,
            IIssueService service,
            IShowIssueDetailsCommand showIssueDetails)
            : base(repositoryService)
        {
            Guard.ArgumentNotNull(service, nameof(service));

            this.service = service;
            this.showIssueDetails = showIssueDetails;

            webUrl = this.WhenAnyValue(x => x.RemoteRepository)
                .Select(x => x?.CloneUrl?.ToRepositoryUrl().Append("issue"))
                .ToProperty(this, x => x.WebUrl);
            OpenItemInBrowser = ReactiveCommand.Create();
        }

        /// <inheritdoc/>
        public override IReadOnlyList<string> States => states;

        /// <inheritdoc/>
        public Uri WebUrl => webUrl.Value;

        /// <inheritdoc/>
        public ReactiveCommand<object> OpenItemInBrowser { get; }

        /// <inheritdoc/>
        protected override IVirtualizingListSource<IIssueListItemViewModelBase> CreateItemSource()
        {
            return new ItemSource(this);
        }

        /// <inheritdoc/>
        protected override Task DoOpenItem(IIssueListItemViewModelBase item)
        {
            var i = (IIssueListItemViewModel)item;
            return showIssueDetails.Execute(new ShowIssueDetailsParams(
                HostAddress.Create(LocalRepository.CloneUrl),
                RemoteRepository.Owner,
                RemoteRepository.Name,
                i.Number));
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

        class ItemSource : SequentialListSource<IssueListItemModel, IIssueListItemViewModelBase>
        {
            readonly IssueListViewModel owner;

            public ItemSource(IssueListViewModel owner)
            {
                this.owner = owner;
            }

            protected override IIssueListItemViewModelBase CreateViewModel(IssueListItemModel model)
            {
                return new IssueListItemViewModel(model);
            }

            protected override async Task<Page<IssueListItemModel>> LoadPage(string after)
            {
                IssueState[] states;

                switch (owner.SelectedState)
                {
                    case "Open":
                        states = new[] { IssueState.Open };
                        break;
                    case "Closed":
                        states = new[] { IssueState.Closed};
                        break;
                    default:
                        states = new[] { IssueState.Open, IssueState.Closed };
                        break;
                }

                var result = await owner.service.ReadIssues(
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
