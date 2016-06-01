namespace GitHub.Services
{
    public interface IUsageTracker
    {
        void IncrementCloneCount();
        void IncrementCommitCount();
        void IncrementSyncCount();
        void IncrementShellLaunchCount();
        void IncrementLaunchCount();
        void IncrementPartialCommitCount();
        void IncrementTutorialRunCount();
        void IncrementOpenInExplorerCount();
        void IncrementOpenInShellCount();
        void IncrementBranchSwitchCount();
        void IncrementDiscardChangesCount();
        void IncrementNumberOfOpenedURLs();
        void IncrementLfsDiffCount();
        void IncrementMergeCommitCount();
        void IncrementMergeConflictCount();
        void IncrementOpenInEditorCount();
        void IncrementUpstreamPullRequestCount();
    }
}
