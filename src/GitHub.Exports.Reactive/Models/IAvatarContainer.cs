using System;

namespace GitHub.Models
{
    /// <summary>
    /// Interface for anything that can have an associated avatar such as a user or organization.
    /// </summary>
    public interface IAvatarContainer
    {
        /// <summary>
        /// Used as the cache key for the avatar.
        /// </summary>
        string Login { get; }

        /// <summary>
        /// URL to the avatar.
        /// </summary>
        string AvatarUrl { get; }

        /// <summary>
        /// Helps determine the default image. Yeah, this interface isn't perfect.
        /// </summary>
        bool IsUser { get; }
    }
}
