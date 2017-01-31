using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHub.Api
{
    public interface ITwoFactorChallengeHandler
    {
        Task<TwoFactorChallengeResult> HandleTwoFactorException(
            TwoFactorAuthorizationException exception);
    }
}
