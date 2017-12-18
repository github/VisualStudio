using System;
using System.Runtime.Serialization;

namespace GitHub.Api
{
    /// <summary>
    /// Thrown when the login for a user does not have the required API scopes.
    /// </summary>
    [Serializable]
    public class IncorrectScopesException : Exception
    {
        public IncorrectScopesException()
            : this("Incorrect API scopes.")
        {
        }

        public IncorrectScopesException(string message)
            : base(message)
        {
        }

        public IncorrectScopesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected IncorrectScopesException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
