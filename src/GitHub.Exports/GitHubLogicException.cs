using System;
using System.Runtime.Serialization;

namespace GitHub
{
    /// <summary>
    /// An exception that signals an error in program logic.
    /// </summary>
    /// <remarks>
    /// This error is often used instead of Debug.Assert because Debug.Assert causes problems with
    /// unit tests and the XAML designer.
    /// </remarks>
    [Serializable]
    public class GitHubLogicException : Exception
    {
        public GitHubLogicException()
        {
        }

        public GitHubLogicException(string message)
            :base(message)
        {
        }

        public GitHubLogicException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected GitHubLogicException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
