using System;
using Octokit;
using GitHub.ViewModels;

namespace GitHub.Authentication
{
    public interface ITwoFactorChallengeHandler
    {
        void SetViewModel(IViewModel vm);
        IViewModel CurrentViewModel { get; }
        IObservable<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorAuthorizationException exception);
    }
}
