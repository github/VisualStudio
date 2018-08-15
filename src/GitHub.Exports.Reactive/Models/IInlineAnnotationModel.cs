namespace GitHub.Models
{
    /// <summary>
    /// Represents an inline annotation on an <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    public interface IInlineAnnotationModel
    {
        /// <summary>
        /// Gets the associated check run
        /// </summary>
        CheckRunModel CheckRun { get; }

        /// <summary>
        /// Gets the annotation
        /// </summary>
        CheckRunAnnotationModel Annotation { get; }
    }
}