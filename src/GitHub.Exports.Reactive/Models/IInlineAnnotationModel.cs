namespace GitHub.Models
{
    /// <summary>
    /// Represents an inline annotation on an <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    public interface IInlineAnnotationModel
    {
        int StartLine { get; }
        int EndLine { get; }
        string Title { get; }
        CheckAnnotationLevel AnnotationLevel { get; }
        string Message { get; }
        string FileName { get; }
        string LineDescription { get; }
        string CheckRunName { get; }
    }
}