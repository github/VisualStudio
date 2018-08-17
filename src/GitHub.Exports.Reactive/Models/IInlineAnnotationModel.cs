namespace GitHub.Models
{
    /// <summary>
    /// Represents an inline annotation on an <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    public interface IInlineAnnotationModel
    {
        int StartLine { get; }
        int EndLine { get; }
        CheckAnnotationLevel AnnotationLevel { get; }
    }
}