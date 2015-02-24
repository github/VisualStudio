using ReactiveUI;
using System;
using System.Globalization;

namespace GitHub.UserErrors
{
    public class PrivateRepositoryQuotaExceededUserError : PublishRepositoryUserError
    {
        public PrivateRepositoryQuotaExceededUserError(IAccount account, string errorMessage, string errorCauseOrResolution = null)
            : base(errorMessage, errorCauseOrResolution)
        {
            UserErrorIcon = StockUserErrorIcon.Error;
            UsedPrivateSlots = account.OwnedPrivateRepos;
            AvaliblePrivateSlots = account.PrivateReposInPlan;
        }

        public long AvaliblePrivateSlots { get; set; }

        public int UsedPrivateSlots { get; set; }

        public static IObservable<RecoveryOptionResult> Throw(IAccount account)
        {
            var errorMessage = string.Format(CultureInfo.InvariantCulture, 
                "You are using {0} out of {1} private repositories.", account.OwnedPrivateRepos, account.PrivateReposInPlan);

            return UserError.Throw(new PrivateRepositoryQuotaExceededUserError(account, errorMessage));
        }
    }
}