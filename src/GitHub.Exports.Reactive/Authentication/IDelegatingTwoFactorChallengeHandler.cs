using System;
using Octokit;
using GitHub.ViewModels;
using GitHub.Api;

namespace GitHub.Authentication
{
    public interface IDelegatingTwoFactorChallengeHandler : ITwoFactorChallengeHandler
    {
        void SetViewModel(IViewModel vm);
        IViewModel CurrentViewModel { get; }
    }
}
