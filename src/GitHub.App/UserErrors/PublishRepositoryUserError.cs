using System;
using System.Diagnostics.CodeAnalysis;
using Octokit;
using ReactiveUI;
using GitHub.Services;

namespace GitHub.UserErrors
{
    public class PublishRepositoryUserError : UserError
    {
        public PublishRepositoryUserError(string errorMessage, string errorCauseOrResolution = null)
            : base(errorMessage, errorCauseOrResolution)
        {
            UserErrorIcon = StockUserErrorIcon.Error;
        }

        public static IObservable<RecoveryOptionResult> Throw(Exception innerException = null)
        {
            var translation = StandardUserErrors.Translator.Value.GetUserErrorTranslation(ErrorType.RepoCreationOnGitHubFailed, innerException);
            return UserError.Throw(new PublishRepositoryUserError(translation.ErrorMessage, translation.CauseOrResolution));
        }

        public static IObservable<RecoveryOptionResult> Throw(string errorMessage, string errorCauseOrResolution = null)
        {
            return UserError.Throw(new PublishRepositoryUserError(errorMessage, errorCauseOrResolution));
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Because no. It's only for that kind of exception")]
        public static void ThrowPrivateQuotaExceeded(PrivateRepositoryQuotaExceededException exception, IAccount account)
        {
            if (account.IsOnFreePlan)
            {
                var translation = StandardUserErrors.Translator.Value.GetUserErrorTranslation(ErrorType.RepoCreationOnGitHubFailed, exception);
                UserError.Throw(new PrivateRepositoryOnFreeAccountUserError(translation.ErrorMessage, translation.CauseOrResolution));
            }
            else
            {
                PrivateRepositoryQuotaExceededUserError.Throw(account);
            }
        }
    }
}