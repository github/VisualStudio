namespace GitHub.Commands
{
    /// <summary>
    /// Navigate from a PR file to the equivalent file and location in the editor (or the reverse).
    /// </summary>
    /// <remarks>
    /// This command will do one of the following depending on context.
    /// Navigate from PR file diff to the working file in the solution.
    /// Navigate from the working file in the solution to the PR file diff.
    /// Navigate from an editable diff (e.g. 'View Changes in Solution') to the editor view.
    /// </remarks>
    public interface IGoToSolutionOrPullRequestFileCommand : IVsCommand
    {
    }
}
