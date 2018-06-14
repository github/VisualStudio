using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using static System.FormattableString;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : IssueListViewModelBase, IPullRequestListViewModel
    {
        readonly IPullRequestService service;

        [ImportingConstructor]
        public PullRequestListViewModel(IPullRequestService service)
        {
            this.service = service;
        }

        protected override IVirtualizingListSource<IViewModel> CreateItemSource()
        {
            return new ItemSource(this);
        }

        protected override Task DoOpenItem(IViewModel item)
        {
            var i = (IPullRequestListItemViewModel)item;
            NavigateTo(Invariant($"{LocalRepository.Owner}/{LocalRepository.Name}/pull/{i.Number}"));
            return Task.CompletedTask;
        }

        class ItemSource : SequentialListSource<PullRequestListItemModel, IViewModel>
        {
            readonly PullRequestListViewModel owner;

            public ItemSource(PullRequestListViewModel owner)
            {
                this.owner = owner;
            }

            protected override IViewModel CreateViewModel(PullRequestListItemModel model)
            {
                return new PullRequestListItemViewModel(model);
            }

            protected override Task<Page<PullRequestListItemModel>> LoadPage(string after)
            {
                return owner.service.ReadPullRequests(
                    HostAddress.Create(owner.LocalRepository.CloneUrl),
                    owner.LocalRepository.Owner,
                    owner.LocalRepository.Name,
                    after);
            }

            protected override void OnBeginLoading()
            {
                owner.IsBusy = true;
            }

            protected override void OnEndLoading()
            {
                owner.IsBusy = false;
            }
        }
    }
}
