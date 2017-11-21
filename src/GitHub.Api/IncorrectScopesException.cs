using System;

namespace GitHub.Api
{
    /// <summary>
    /// Thrown when the login for a user does not have the required scopes.
    /// </summary>
    public class IncorrectScopesException : Exception
    {
        public IncorrectScopesException()
            : this("You need to sign out and back in.")
        {
        }

        public IncorrectScopesException(string message)
            : base(message)
        {
        }
    }
}
