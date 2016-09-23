using System;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public enum ChangedFilesView
    {
        TreeView,
        ListView,
    }

    public enum OpenChangedFileAction
    {
        Diff,
        Open
    }

    public interface IPullRequestDetailViewModel : IViewModel, IHasBusy
    {
        PullRequestState State { get; }
        string SourceBranchDisplayName { get; }
        string TargetBranchDisplayName { get; }
        int CommitCount { get; }
        int FilesChangedCount { get; }
        IAccount Author { get; }
        DateTimeOffset CreatedAt { get; }
        int Number { get; }
        string Body { get; }
        int ChangedFilesCount { get; }
        ChangedFilesView ChangedFilesView { get; set; }
        OpenChangedFileAction OpenChangedFileAction { get; set; }
        IReactiveList<IPullRequestChangeNode> ChangedFilesTree { get; }
        IReactiveList<IPullRequestFileViewModel> ChangedFilesList { get; }

        ReactiveCommand<object> OpenOnGitHub { get; }
        ReactiveCommand<object> ToggleChangedFilesView { get; }
        ReactiveCommand<object> ToggleOpenChangedFileAction { get; }
    }
}
