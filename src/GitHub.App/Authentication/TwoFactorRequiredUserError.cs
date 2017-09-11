using System;
using GitHub.Extensions;
using Octokit;
using ReactiveUI;

namespace GitHub.Authentication
{
    public class TwoFactorRequiredUserError : UserError
    {
        public TwoFactorRequiredUserError(TwoFactorAuthorizationException exception)
            : base(exception.Message, innerException: exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            TwoFactorType = exception.TwoFactorType;
            RetryFailed = exception is TwoFactorChallengeFailedException;
        }

        public bool RetryFailed { get; private set; }

        public TwoFactorType TwoFactorType { get; private set; }

        public IObservable<RecoveryOptionResult> Throw()
        {
            return Throw(this);
        }
    }
}
