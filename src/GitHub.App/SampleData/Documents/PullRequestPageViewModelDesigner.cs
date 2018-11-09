using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Documents;
using ReactiveUI;

namespace GitHub.SampleData.Documents
{
    public class PullRequestPageViewModelDesigner : ViewModelBase, IPullRequestPageViewModel
    {
        public PullRequestPageViewModelDesigner()
        {
            Body = @"Save drafts of inline comments, PR reviews and PRs.

> Note: This feature required a refactoring of the comment view models because they now need async initialization and to be available from GitHub.App. This part of the PR has been submitted separately as #1993 to ease review. The two PRs can alternatively be reviewed as one if that's more convenient.

As described in #1905, it is easy to lose a comment that you're working on if you close the diff view accidentally. This PR saves drafts of comments as they are being written to an SQLite database.

In addition to saving drafts of inline comments, it also saves comments to PR reviews and PRs themselves.

The comments are written to an SQLite database directly instead of going through Akavache because in the case of inline reviews, there can be many drafts in progress on a separate file. When a diff is opened we need to look for any comments present on that file and show the most recent. That use-case didn't fit well with Akavache (being a pure key/value store).

## Testing

### Inline Comments

- Open a PR
- Open the diff of a file
- Start adding a comment
- Close the comment by closing the peek view, or the document tab
- Reopen the diff
- You should see the comment displayed in edit mode with the draft of the comment you were previously writing

### PR reviews

- Open a PR
- Click ""Add your review""
- Start adding a review
- Click the ""Back"" button and navigate to a different PR
- Click the ""Back"" button and navigate to the original PR
- Click ""Add your review""
- You should see the the draft of the review you were previously writing

### PRs

-Click ""Create new"" at the top of the PR list
- Start adding a PR title/ description
- Close VS
- Restart VS and click ""Create new"" again
- You should see the the draft of the PR you were previously writing

Depends on #1993 
Fixes #1905";
            Timeline = new IViewModel[]
            {
                new CommitSummariesViewModel(
                    new CommitSummaryViewModel(new CommitModel
                    {
                        Author = new ActorModel { Login = "grokys" },
                        AbbreviatedOid = "c7c7d25",
                        MessageHeadline = "Refactor comment view models."
                    }),
                    new CommitSummaryViewModel(new CommitModel
                    {
                        Author = new ActorModel { Login = "grokys" },
                        AbbreviatedOid = "04e6a90",
                        MessageHeadline = "Refactor comment view models.",
                    })),
                new CommentViewModelDesigner
                {
                    Author = new ActorViewModelDesigner("meaghanlewis"),
                    Body = @"This is looking great! Really enjoying using this feature so far.

When leaving an inline comment, the comment posts successfully and then a new comment is drafted with the same text.",
                },
                new CommentViewModelDesigner
                {
                    Author = new ActorViewModelDesigner("grokys"),
                    Body = @"Oops, sorry about that @meaghanlewis - I was sure I tested those things, but must have got messed up again at some point. Should be fixed now.",
                },
            };
        }

        public string Id { get; set; }
        public PullRequestState State { get; set; } = PullRequestState.Open;
        public IReadOnlyList<IViewModel> Timeline { get; }
        public string SourceBranchDisplayName { get; set; } = "feature/save-drafts";
        public string TargetBranchDisplayName { get; set; } = "master";
        public IActorViewModel Author { get; set; } = new ActorViewModelDesigner("grokys");
        public int CommitCount { get; set; } = 2;
        public string Body { get; set; }
        public int Number { get; set; } = 1994;
        public LocalRepositoryModel LocalRepository { get; }
        public RemoteRepositoryModel Repository { get; set; }
        public string Title { get; set; } = "Save drafts of comments";
        public Uri WebUrl { get; set; }
        public ReactiveCommand<Unit, Unit> OpenOnGitHub { get; }
        public ReactiveCommand<string, Unit> ShowCommit { get; }


        public Task InitializeAsync(RemoteRepositoryModel repository, LocalRepositoryModel localRepository, ActorModel currentUser, PullRequestDetailModel model)
        {
            throw new NotImplementedException();
        }
    }
}
