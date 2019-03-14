namespace GitHub.Models
{
    /// <summary>
    /// Represents a commit actor (which may or may not have an associated User or Bot).
    /// </summary>
    public class CommitActorModel
    {
        /// <summary>
        /// Gets or sets the actor user
        /// </summary>
        public ActorModel User { get; set; }

        /// <summary>
        /// Gets or sets the actor name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the actor email
        /// </summary>
        public string Email { get; set; }
    }
}