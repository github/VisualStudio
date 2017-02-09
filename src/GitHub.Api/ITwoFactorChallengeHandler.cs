using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHub.Api
{
    /// <summary>
    /// Interface for handling Two Factor Authentication challenges in a
    /// <see cref="LoginManager"/> operation.
    /// </summary>
    public interface ITwoFactorChallengeHandler
    {
        /// <summary>
        /// Called when the GitHub API responds to a login request with a 2FA challenge.
        /// </summary>
        /// <param name="exception">The 2FA exception that initiated the challenge.</param>
        /// <returns>A task returning a <see cref="TwoFactorChallengeResult"/>.</returns>
        Task<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorAuthorizationException exception);

        /// <summary>
        /// Called when an error occurs sending the 2FA challenge response.
        /// </summary>
        /// <param name="e">The exception that occurred.</param>
        /// <remarks>
        /// This method is called when, on sending the challenge response returned by 
        /// <see cref="HandleTwoFactorException(TwoFactorAuthorizationException)"/>, an exception of
        /// a type other than <see cref="TwoFactorAuthorizationException"/> is thrown. This
        /// indicates that the login attempt is over and any 2FA dialog being shown should close.
        /// </remarks>
        Task ChallengeFailed(Exception e);
    }
}
