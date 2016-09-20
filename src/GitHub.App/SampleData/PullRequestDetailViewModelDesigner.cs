using System;
using System.Diagnostics.CodeAnalysis;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestDetailViewModelDesigner : BaseViewModel, IPullRequestDetailViewModel
    {
        public PullRequestDetailViewModelDesigner()
        {
            Title = "Error handling/bubbling from viewmodels to views to viewhosts";
            State = new PullRequestState { Name = "Open", IsOpen = true };
            SourceBranchDisplayName = "shana/error-handling";
            TargetBranchDisplayName = "master";
            CommitCount = 9;
            FilesChangedCount = 4;
            Author = new AccountDesigner { Login = "shana", IsUser = true };
            CreatedAt = DateTime.Now.Subtract(TimeSpan.FromDays(3));
            Number = 419;
            Body = @"Adds a way to surface errors from the view model to the view so that view hosts can get to them.

ViewModels are responsible for handling the UI on the view they control, but they shouldn't be handling UI for things outside of the view. In this case, we're showing errors in VS outside the view, and that should be handled by the section that is hosting the view.

This requires that errors be propagated from the viewmodel to the view and from there to the host via the IView interface, since hosts don't usually know what they're hosting.";
        }

        public PullRequestState State { get; set; }
        public string SourceBranchDisplayName { get; set; }
        public string TargetBranchDisplayName { get; set; }
        public int CommitCount { get; set; }
        public int FilesChangedCount { get; set; }
        public IAccount Author { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int Number { get; set; }
        public string Body { get; set; }

        public ReactiveCommand<object> OpenOnGitHub { get; }
    }
}