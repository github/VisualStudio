namespace GitHub.Commands
{
    /// <summary>
    /// This appears as the named command `GitHub.OpenFromUrl` and must be bound to a keyboard shortcut or executed
    /// via the `Command Window`. In future it will appear on `File > Open > Open from GitHub`.
    /// 
    /// When executed it will offer to clone, open and navigate to the file pointed to by a URL in the clipboard.
    /// This spike uses Yes/No/Cancel dialogs, but the final version will use a UI to control what action is performed
    /// and allow the user to override the default repository location.
    /// </summary>
    public interface IOpenFromUrlCommand : IVsCommand<string>
    {
    }
}