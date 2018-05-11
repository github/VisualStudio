using System;
using System.Windows.Input;
using GitHub.Commands;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    public class PullRequestFileMarginViewModel : ReactiveObject
    {
        bool enabled;
        string fileName;
        int commentsInFile;
        bool marginEnabled;

        public PullRequestFileMarginViewModel(ICommand toggleInlineCommentMarginCommand, ICommand viewChangesCommand,
            Lazy<IUsageTracker> usageTracker)
        {
            ToggleInlineCommentMarginCommand = toggleInlineCommentMarginCommand = new UsageTrackingCommand(
                usageTracker, x => x.NumberOfPullRequestFileMarginToggleInlineCommentMargin, toggleInlineCommentMarginCommand);
            ViewChangesCommand = viewChangesCommand = new UsageTrackingCommand(
                usageTracker, x => x.NumberOfPullRequestFileMarginViewChanges, viewChangesCommand);
        }

        public bool Enabled
        {
            get { return enabled; }
            set { this.RaiseAndSetIfChanged(ref enabled, value); }
        }

        public string FileName
        {
            get { return fileName; }
            set { this.RaiseAndSetIfChanged(ref fileName, value); }
        }

        public int CommentsInFile
        {
            get { return commentsInFile; }
            set { this.RaiseAndSetIfChanged(ref commentsInFile, value); }
        }

        public bool MarginEnabled
        {
            get { return marginEnabled; }
            set { this.RaiseAndSetIfChanged(ref marginEnabled, value); }
        }

        public ICommand ToggleInlineCommentMarginCommand { get; }

        public ICommand ViewChangesCommand { get; }
    }
}
