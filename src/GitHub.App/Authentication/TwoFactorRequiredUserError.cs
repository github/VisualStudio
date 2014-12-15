using System;
using NullGuard;
using Octokit;
using ReactiveUI;

namespace GitHub.Authentication
{
    public class TwoFactorRequiredUserError : UserError
    {
        public TwoFactorRequiredUserError(TwoFactorRequiredException exception)
            : base(exception.Message, innerException: exception)
        {
            TwoFactorType = exception.TwoFactorType;
        }

        public TwoFactorType TwoFactorType { get; private set; }

        [AllowNull]
        public TwoFactorChallengeResult ChallengeResult
        {
            [return: AllowNull]
            get;
            set;
        }

        public IObservable<RecoveryOptionResult> Throw()
        {
            return Throw(this);
        }
    }
}
