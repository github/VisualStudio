using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
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
        static readonly IReadOnlyList<string> states = new[] { "Open", "Closed", "All" };
        readonly IPullRequestService service;

        [ImportingConstructor]
        public PullRequestListViewModel(IPullRequestService service)
        {
            this.service = service;
        }

        public override IReadOnlyList<string> States => states;

        protected override IVirtualizingListSource<IViewModel> CreateItemSource(CancellationToken cancel)
        {
            return new ItemSource(this, cancel);
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

            public ItemSource(PullRequestListViewModel owner, CancellationToken cancel)
                : base(cancel)
            {
                this.owner = owner;
            }

            protected override IViewModel CreateViewModel(PullRequestListItemModel model)
            {
                return new PullRequestListItemViewModel(model);
            }

            protected override Task<Page<PullRequestListItemModel>> LoadPage(string after)
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

                var sw = Stopwatch.StartNew();
                var result = owner.service.ReadPullRequests(
                    HostAddress.Create(owner.LocalRepository.CloneUrl),
                    owner.LocalRepository.Owner,
                    owner.LocalRepository.Name,
                    after,
                    states);
                sw.Stop();
                System.Diagnostics.Debug.WriteLine("Read PR page in " + sw.Elapsed);
                return result;
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
