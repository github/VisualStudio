using System;
using Octokit;
using GitHub.ViewModels;

namespace GitHub.Authentication
{
    public interface ITwoFactorChallengeHandler
    {
        void SetViewModel(ITwoFactorDialogViewModel vm);
        IObservable<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorRequiredException exception);
    }
}
