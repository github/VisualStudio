using System;
using GitHub.Extensions;
using Octokit;
using ReactiveUI;

namespace GitHub.Authentication
{
    public class TwoFactorRequiredUserError : UserError
    {
        public TwoFactorRequiredUserError(TwoFactorAuthorizationException exception)
            : this(exception, exception.TwoFactorType)
        {
        }

        public TwoFactorRequiredUserError(
            TwoFactorAuthorizationException exception,
            TwoFactorType twoFactorType)
            : base(exception.Message, innerException: exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            TwoFactorType = twoFactorType;
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
