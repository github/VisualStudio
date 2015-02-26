using System;
using Octokit;

namespace GitHub.Authentication
{
    public interface ITwoFactorChallengeHandler
    {
        IObservable<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorRequiredException exception);
    }
}
