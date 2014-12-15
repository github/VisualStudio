using System;
using System.Reactive.Linq;
using System.Windows;
using ReactiveUI;

namespace GitHub.Services
{
    public enum ErrorType
    {
        BranchCreateFailed,
        BranchDeleteFailed,
        BranchCheckoutFailed,
        BranchPublishFailed,
        BranchUnpublishFailed,
        BranchListFailed,
        CannotDropFolder,
        ClipboardFailed,
        ClonedFailed,
        CloneFailedNotLoggedIn,
        CommitCreateFailed,
        CommitRevertFailed,
        CommitUndoFailed,
        CommitFilesLoadFailed,
        DeleteEnterpriseServerFailed,
        DiscardFileChangesFailed,
        DiscardAllChangesFailed,
        IgnoreFileFailed,
        EnterpriseConnectFailed,
        GitExtractionFailed,
        GettingHeadFailed,
        LaunchEnterpriseConnectionFailed,
        LoginFailed,
        LogFileError,
        RepoCorrupted,
        RepoDirectoryAlreadyExists,
        RepoCreationFailed,
        RepoCreationOnGitHubFailed,
        RepoCreationAsPrivateNotAvailableForFreePlan,
        RepoExistsOnDisk,
        RepoExistsForUser,
        RepoExistsInOrganization,
        RepositoryNotFoundOnDisk,
        SyncFailed,
        ShellFailed,
        CustomShellFailed,
        WorkingDirectoryDoesNotExist,
        MergeFailed,
        PowerShellNotFound,
        LoadingCommitsFailed,
        LoadingWorkingDirectoryFailed,
        SaveRepositorySettingsFailed,
        MenuActionFailed,
        Global,
        RefreshFailed
    }

    public static class StandardUserErrors
    {
        public static IObservable<RecoveryOptionResult> ShowUserErrorMessage(
            this Exception ex, ErrorType errorType, params object[] messageArgs)
        {
            // TODO: Fix this. This is just placeholder logic. -@haacked
            Console.WriteLine(errorType);
            Console.WriteLine(messageArgs);
            MessageBox.Show(ex.Message);
            return Observable.Return(new RecoveryOptionResult());
        }
    }
}
