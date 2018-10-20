using ReactiveUI;
using ReactiveUI.Legacy;

#pragma warning disable CS0618 // Type or member is obsolete

namespace GitHub.UserErrors
{
    public class PrivateRepositoryOnFreeAccountUserError : PublishRepositoryUserError
    {
        public PrivateRepositoryOnFreeAccountUserError(string errorMessage, string errorCauseOrResolution = null)
            : base(errorMessage, errorCauseOrResolution)
        {
            UserErrorIcon = StockUserErrorIcon.Error;
        }
    }
}