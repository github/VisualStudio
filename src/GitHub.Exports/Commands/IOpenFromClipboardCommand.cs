namespace GitHub.Commands
{
    /// <summary>
    /// Open a file in the current repository based on a URL in the clipboard.
    /// </summary>
    /// <remarks>
    /// This command appears as `Code context > GitHub > Open from clipboard`.
    /// 
    /// Open a working directory file at the same location as a GitHub URL in the clipboard. If the URL links to
    /// a line or range of lines, these lines will be selected. If the working directory file is different to the
    /// target file, the target file will be opened in the `Blame (Annotate)` view. If the URL is from a different
    /// fork, it will still open the target file assuming that the target commit/branch exists.
    /// Currently only GitHub `blob` URLs are supported. In a future version we can add support for `pull`, `issue`,
    /// `tree`, `commits`, `blame` and any other URL types that might make sense.
    /// </remarks>
    public interface IOpenFromClipboardCommand : IVsCommand<string>
    {
    }
}