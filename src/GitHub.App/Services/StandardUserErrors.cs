using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using GitHub.Extensions;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;
using ReactiveUI;
using ReactiveUI.Legacy;

#pragma warning disable CS0618 // Type or member is obsolete

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
        CannotDropFolderUnauthorizedAccess,
        ClipboardFailed,
        CloneOrOpenFailed,
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
        LogoutFailed,
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
        DefaultClonePathInvalid,
        MergeFailed,
        PowerShellNotFound,
        LoadingCommitsFailed,
        LoadingWorkingDirectoryFailed,
        SaveRepositorySettingsFailed,
        MenuActionFailed,
        Global,
        RefreshFailed,
        GistCreateFailed,
        RepoForkFailed
    }

    public static class StandardUserErrors
    {
        internal static readonly Lazy<ErrorMessageTranslator> Translator = new Lazy<ErrorMessageTranslator>(() => new ErrorMessageTranslator(new Dictionary<ErrorType, ErrorMap>
        {
            {
                // Exceptions are matched against global *FIRST* before they are matched against 
                // specific operations.
                ErrorType.Global, Map(null,
                    new Translation("Cannot find config file '(.*?)'", "The Git configuration file is missing.", "The global Git configuration file '$1' could not be found. Please open the options menu from the dashboard and update your name and email settings to create a new one."),
                    new Translation<AuthorizationException>("Authentication failed", "Your credentials may be out of date. Please log out of the application and then log back in before retrying the operation."),
                    new Translation<LoginAttemptsExceededException>("Maximum login attempts exceeded", "Please log out of the application and then log back in before retrying the operation. You may need to wait a few minutes."),
                    new Translation("fatal: Authentication failed", "Authentication failed", "Your credentials may be out of date. Please log out of the application and then log back in before retrying the operation."),
                    new Translation(@"({""message"":""Bad credentials.?""}|Bad credentials.?)", "Authentication failed", "Your credentials may be out of date. Please log out of the application and then log back in before retrying the operation."),
                    new Translation<DirectoryNotFoundException>("Directory not found", "The directory does not exist"),
                    new Translation<UnauthorizedAccessException>("Access denied", "You do not have permissions to access this file or folder"),
                    new Translation("EPOLICYKEYAGE", "Unverified SSH Key", "{0}", ParseUnverifiedSshKeyMessageFromExceptionMessage))
            },
            {
                ErrorType.BranchCreateFailed, Map(Defaults("Failed to create new branch"),
                    new Translation("No valid git object identified by '.+?' exists in the repository.", "Failed to create branch", "The current branch doesn’t have any commits."))
            },
            {
                ErrorType.BranchDeleteFailed, Map(Defaults("error", "Failed to delete the branch."),
                    new Translation(@"fatal: .*? not found: did you run git update-server-info on the server\?",
                        "Failed to delete the branch",
                        "Please make sure the repository exists and that you have permissions to change it."),
                    new Translation("error: The requested URL returned error: 403 while accessing .*",
                        "Failed to delete the branch",
                        "Please make sure the repository exists and that you have permissions to change it."),
                    new Translation("fatal: Could not read from remote repository.",
                        "Failed to delete the branch",
                        "Please make sure the repository exists and that you have permissions to change it."),
                    new Translation(@".*?\(deletion of the current branch prohibited\).*",
                        "Cannot delete the default branch",
                        "To delete this branch, log in to " + HostAddress.GitHubDotComHostAddress.WebUri.Host + " and change " +
                        "the repository’s default branch to another branch first."))
            },
            {
                ErrorType.BranchUnpublishFailed, Map(Defaults("error", "Failed to unpublish the branch."),
                    new Translation(@"fatal: .*? not found: did you run git update-server-info on the server\?",
                        "Failed to unpublish the branch",
                        "Please make sure the repository exists and that you have permissions to change it."),
                    new Translation("error: The requested URL returned error: 403 while accessing .*",
                        "Failed to unpublish the branch",
                        "Please make sure the repository exists and that you have permissions to change it."),
                    new Translation("fatal: Could not read from remote repository.",
                        "Failed to unpublish the branch",
                        "Please make sure the repository exists and that you have permissions to change it."),
                    new Translation(@".*?\(deletion of the current branch prohibited\).*",
                        "Cannot unpublish the default branch",
                        "To unpublish this branch, log in to " + HostAddress.GitHubDotComHostAddress.WebUri.Host + " and change " +
                        "the repository’s default branch to another branch first."))
            },
            { ErrorType.ClipboardFailed, Map(Defaults("Failed to copy text to the clipboard.")) },
            {
                ErrorType.CloneOrOpenFailed, Map(Defaults("Failed to clone or open the repository '{0}'", "Email support@github.com if you continue to have problems."),
                    new[]
                    {
                        new Translation(@"fatal: bad config file line (\d+) in (.+)", "Failed to clone the repository '{0}'", @"The config file '$2' is corrupted at line $1. You may need to open the file and try to fix any errors."),
                        new Translation("Process timed out", "Failed to clone the repository '{0}'", "The process timed out. The repository is in an unknown state and likely corrupted. Try deleting it and cloning again."),
                        new Translation("Local inaccessible repositories already exist.", "Failed to clone the repository '{0}'", "Local directories with this repository’s name already exist but can’t be accessed. Try deleting them and trying again."),
                        new Translation("Repo directory '(.*?)' already exists.", "Failed to clone the repository '{0}'", "Could not clone the repository '{0}' because the directory\n'$1' already exists and isn’t empty."),
                        new Translation("Local clone at '(.*?)' is corrupted", "Failed to clone the repository '{0}'", "Failed to clone the repository because a local one exists already at '$1', but is corrupted.  You will need to open a shell to debug the state of this repo."),
                        new Translation("Your local changes to the following files would be overwritten by checkout", "Failed to check out branch", "Failed to check out branch because because local changes would be overwritten. Try committing changes and then checking out the branch again")
                    })
            },
            { ErrorType.CloneFailedNotLoggedIn, Map(Defaults("Clone failed", "Please login to your account before attempting to clone this repository.")) },
            { ErrorType.EnterpriseConnectFailed, Map(Defaults("Connecting to GitHub Enterprise instance failed", "Could not find a GitHub Enterprise instance at '{0}'. Double check the URL and your internet/intranet connection.")) },
            { ErrorType.LaunchEnterpriseConnectionFailed, Map(Defaults("Failed to launch the enterprise connection.")) },
            { ErrorType.LogFileError, Map(Defaults("Could not open the log file", "Could not find or open the log file.")) },
            { ErrorType.LoginFailed, Map(Defaults("Login failed", "Unable to retrieve your user info from the server. A proxy server might be interfering with the request.")) },
            { ErrorType.LogoutFailed, Map(Defaults("Logout failed", "Logout failed. A proxy server might be interfering with the request.")) },
            { ErrorType.RepoCreationAsPrivateNotAvailableForFreePlan, Map(Defaults("Failed to create private repository", "You are currently on a free plan and unable to create private repositories. Either make the repository public or upgrade your account on the website to a plan that allows for private repositories.")) },
            { ErrorType.RepoCreationFailed, Map(Defaults("Failed to create repository", "An error occurred while creating the repository. You might need to open a shell and debug the state of this repo.")) },
            { ErrorType.RepoExistsOnDisk, Map(Defaults("Failed to create repository", "A repository named '{0}' exists in the directory\n'{1}'.")) },
            { ErrorType.RepositoryNotFoundOnDisk, Map(Defaults("Repository not found", "Could not find the repository '{0}' in the location '{1}'.\nDid you move it somewhere else on your filesystem?")) },
            { ErrorType.RepoExistsForUser, Map(Defaults("Failed to create repository", "A repository named '{0}' already exists in your GitHub account.")) },
            { ErrorType.RepoExistsInOrganization, Map(Defaults("Failed to create repository", "A repository named '{0}' exists in the organization '{1}'.")) },
            { ErrorType.LoadingWorkingDirectoryFailed, Map(Defaults("Failed to refresh the working directory", "You might need to open a shell and debug the state of this repo.")) },
            {
                ErrorType.RefreshFailed, Map(Defaults("Refresh failed", "Refresh failed unexpectedly. Please email support@github.com if this error persists."),
                    new Translation<HttpRequestException>("Refresh failed", "Could not connect to the remote server. The server or your internect connection could be down")) },
            { ErrorType.GistCreateFailed, Map(Defaults("Failed to create gist", "Creating a gist failed unexpectedly. Try logging back in.")) },
            { ErrorType.RepoForkFailed, Map(Defaults("Failed to create fork")) },
        }));

        public static string GetUserFriendlyErrorMessage(this Exception exception, ErrorType errorType, params object[] messageArgs)
        {
            var translation = exception.GetUserFriendlyError(errorType, messageArgs);
            if (translation == null) return exception.Message;
            return translation.ErrorMessage + Environment.NewLine + translation.ErrorCauseOrResolution;
        }

        public static IObservable<RecoveryOptionResult> ShowUserErrorMessage(this Exception exception, ErrorType errorType, params object[] messageArgs)
        {
            return exception.DisplayErrorMessage(errorType, messageArgs, null);
        }

        public static IObservable<RecoveryOptionResult> ShowUserErrorMessage(ErrorType errorType, params object[] messageArgs)
        {
            return DisplayErrorMessage(null, errorType, messageArgs, null);
        }

        public static IObservable<RecoveryOptionResult> ShowUserThatRepoAlreadyExists(string repositoryName, string fullPath)
        {
            return DisplayErrorMessage(ErrorType.RepoExistsOnDisk, new object[] { repositoryName, fullPath }, new[] { OpenPathInExplorer(fullPath), Cancel });
        }

        public static IObservable<RecoveryOptionResult> ShowUserErrorThatRequiresNavigatingToBilling(
            this Exception exception,
            IAccount account)
        {
            var errorType = (exception is PrivateRepositoryQuotaExceededException && account.IsOnFreePlan)
                ? ErrorType.RepoCreationAsPrivateNotAvailableForFreePlan
                : ErrorType.RepoCreationOnGitHubFailed;

            return exception.DisplayErrorMessage(
                errorType,
                Array.Empty<object>(),
                new[] { OpenBrowser("View Plans", account.Billing()), Cancel });
        }

        static IObservable<RecoveryOptionResult> DisplayErrorMessage(ErrorType errorType, object[] messageArgs, IEnumerable<IRecoveryCommand> recoveryOptions)
        {
            return DisplayErrorMessage(null, errorType, messageArgs, recoveryOptions);
        }

        static IObservable<RecoveryOptionResult> DisplayErrorMessage(this Exception exception, ErrorType errorType, object[] messageArgs, IEnumerable<IRecoveryCommand> recoveryOptions)
        {
            var userError = exception.GetUserFriendlyError(errorType, messageArgs);

            if (recoveryOptions != null)
            {
                userError.RecoveryOptions.AddRange(recoveryOptions);
            }
            if (!userError.RecoveryOptions.Any())
                userError.RecoveryOptions.Add(Ok);

            return userError.Throw();
        }

        public static UserError GetUserFriendlyError(this Exception exception, ErrorType errorType, params object[] messageArgs)
        {
            return Translator.Value.GetUserError(errorType, exception, messageArgs);
        }

        public static string ParseUnverifiedSshKeyMessageFromExceptionMessage(Exception exception)
        {
            var index = exception.Message.IndexOf("[EPOLICYKEYAGE]", StringComparison.OrdinalIgnoreCase);
            return index != -1 ?
                exception.Message.Remove(index).Trim()
                : exception.Message;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static IRecoveryCommand Ok
        {
            get
            {
                return new RecoveryCommandWithIcon("OK", "check", x => RecoveryOptionResult.CancelOperation)
                {
                    IsDefault = true,
                    IsCancel = true
                };
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static IRecoveryCommand Cancel
        {
            get
            {
                return new RecoveryCommandWithIcon("Cancel", "x", x => RecoveryOptionResult.CancelOperation)
                {
                    IsCancel = true
                };
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static IRecoveryCommand OpenPathInExplorer(string path)
        {
            return new RecoveryCommandWithIcon("Open in Explorer", "file_directory", x =>
            {
                Process.Start(path);
                return RecoveryOptionResult.CancelOperation;
            });
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "url")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static IRecoveryCommand OpenBrowser(string text, string url)
        {
            return new RecoveryCommandWithIcon(text, "link_external", x =>
            {
                //IoC.Get<IBrowser>().OpenUrl(url);
                return RecoveryOptionResult.CancelOperation;
            })
            {
                IsDefault = true,
            };
        }

        static IObservable<RecoveryOptionResult> Throw(this UserError error)
        {
            //log.WarnException("Showing user error " + error.ErrorCauseOrResolution, error.InnerException);

            return UserError.Throw(error);
        }

        static ErrorMessage Defaults(string heading, string description)
        {
            return new ErrorMessage(heading, description);
        }

        static ErrorMessage Defaults(string description)
        {
            return new ErrorMessage("error", description);
        }

        static ErrorMap Map(ErrorMessage defaultMessage, params Translation[] translations)
        {
            return new ErrorMap(defaultMessage, translations, null);
        }
    }
}
