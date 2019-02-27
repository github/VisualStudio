namespace GitHub.Models
{
    /// <summary>
    /// Model for a single check annotation.
    /// </summary>
    public class CheckRunAnnotationModel
    {
        /// <summary>
        /// The starting 1-based line number (1 indexed).
        /// </summary>
        public int StartLine { get; set; }

        /// <summary>
        /// The ending 1-based line number (1 indexed).
        /// </summary>
        public int EndLine { get; set; }

        /// <summary>
        /// The path that this annotation was made on.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The annotation's message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The annotation's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The annotation's severity level.
        /// </summary>
        public CheckAnnotationLevel AnnotationLevel { get; set; }
    }
}