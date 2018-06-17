using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
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
            CreatePullRequest = ReactiveCommand.Create().OnExecuteCompleted(_ => NavigateTo("pull/new"));
        }

        public override IReadOnlyList<string> States => states;

        public ReactiveCommand<object> CreatePullRequest { get; }

        protected override IVirtualizingListSource<IIssueListItemViewModelBase> CreateItemSource()
        {
            return new ItemSource(this);
        }

        protected override Task DoOpenItem(IViewModel item)
        {
            var i = (IPullRequestListItemViewModel)item;
            NavigateTo(Invariant($"{LocalRepository.Owner}/{LocalRepository.Name}/pull/{i.Number}"));
            return Task.CompletedTask;
        }

        protected override Task<Page<ActorModel>> LoadAuthors(string after)
        {
            return service.ReadAssignableUsers(
                HostAddress.Create(LocalRepository.CloneUrl),
                LocalRepository.Owner,
                LocalRepository.Name,
                after);
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
        }
    }
}
