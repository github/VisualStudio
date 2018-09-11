namespace GitHub.Models
{
    /// <summary>
    /// Represents an inline annotation on an <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    public interface IInlineAnnotationModel
    {
        /// <summary>
        /// Gets the start line of the annotation.
        /// </summary>
        int StartLine { get; }

        /// <summary>
        /// Gets the end line of the annotation.
        /// </summary>
        int EndLine { get; }

        /// <summary>
        /// Gets the annotation level.
        /// </summary>
        CheckAnnotationLevel AnnotationLevel { get; }

        /// <summary>
        /// Gets the annotation title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the annotation message.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets the annotation path.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the a descriptor for the line(s) reported.
        /// </summary>
        string LineDescription { get; }

        /// <summary>
        /// Gets the name of the check run.
        /// </summary>
        string CheckRunName { get; }

        /// <summary>
        /// Gets the sha the check run was created on.
        /// </summary>
        string HeadSha { get; }
    }
}