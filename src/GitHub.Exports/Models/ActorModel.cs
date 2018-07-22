using System;

namespace GitHub.Models
{
    /// <summary>
    /// Represents an actor (a User or a Bot).
    /// </summary>
    public class ActorModel
    {
        /// <summary>
        /// Gets or sets the URL of the actor's avatar.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the actor's login.
        /// </summary>
        public string Login { get; set; }
    }
}
