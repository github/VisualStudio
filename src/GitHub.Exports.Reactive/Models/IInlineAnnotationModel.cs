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

        string Title { get; }
        string Message { get; }
        string FileName { get; }
        string LineDescription { get; }
        string CheckRunName { get; }
        string HeadSha { get; }
    }
}