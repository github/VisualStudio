using ReactiveUI;

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