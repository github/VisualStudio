namespace GitHub.Models
{
    /// <summary>
    /// Model for a single pull request Status.
    /// </summary>
    public class StatusModel
    {
        /// <summary>
        /// The state of the Status
        /// </summary>
        public StatusState State { get; set; }

        /// <summary>
        /// The Status context or title
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// The url where more information about the Status can be found
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// The descritption for the Status
        /// </summary>
        public string Description { get; set; }

        public bool IsRequired { get; set; }
    }
}