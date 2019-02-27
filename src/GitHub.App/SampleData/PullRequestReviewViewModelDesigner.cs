using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestReviewViewModelDesigner : PanePageViewModelBase, IPullRequestReviewViewModel
    {
        public PullRequestReviewViewModelDesigner()
        {
            PullRequestModel = new PullRequestDetailModel
            {
                Number = 419,
                Title = "Fix a ton of potential crashers, odd code and redundant calls in ModelService",
                Author = new ActorModel { Login = "Haacked" },
                UpdatedAt = DateTimeOffset.Now - TimeSpan.FromDays(2),
            };

            Model = new PullRequestReviewModel
            {

                SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(1),
                Author = new ActorModel { Login = "Haacked" },
            };

            Body = @"Just a few comments. I don't feel too strongly about them though.

Otherwise, very nice work here! ✨";

            StateDisplay = "approved";

            FileComments = new[]
            {
                new PullRequestReviewFileCommentViewModelDesigner
                {
                    Body = @"These should probably be properties. Most likely they should be readonly properties. I know that makes creating instances of these not look as nice as using property initializers when constructing an instance, but if these properties should never be mutated after construction, then it guides future consumers to the right behavior.

However, if you're two-way binding these properties to a UI, then ignore the readonly part and make them properties. But in that case they should probably be reactive properties (or implement INPC).",
                    RelativePath = "src/GitHub.Exports.Reactive/ViewModels/IPullRequestListViewModel.cs",
                },
                new PullRequestReviewFileCommentViewModelDesigner
                {
                    Body = "While I have no problems with naming a variable ass I think we should probably avoid swear words in case Microsoft runs their Policheck tool against this code.",
                    RelativePath = "src/GitHub.App/ViewModels/PullRequestListViewModel.cs",
                },
            };

            OutdatedFileComments = new[]
            {
                new PullRequestReviewFileCommentViewModelDesigner
                {
                    Body = @"So this is just casting a mutable list to an IReadOnlyList which can be cast back to List. I know we probably won't do that, but I'm thinking of the next person to come along. The safe thing to do is to wrap List with a ReadOnlyList. We have an extension method ToReadOnlyList for observables. Wouldn't be hard to write one for IEnumerable.",
                    RelativePath = "src/GitHub.Exports.Reactive/ViewModels/IPullRequestListViewModel.cs",
                },
            };
        }

        public string Body { get; }
        public IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments { get; set; }
        public bool IsExpanded { get; set; }
        public bool HasDetails { get; set; }
        public LocalRepositoryModel LocalRepository { get; set; }
        public PullRequestReviewModel Model { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }
        public IReadOnlyList<IPullRequestReviewFileCommentViewModel> OutdatedFileComments { get; set; }
        public PullRequestDetailModel PullRequestModel { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public string StateDisplay { get; set; }
    }
}
