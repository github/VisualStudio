namespace GitHub.VisualStudio.Essentials
{
    /// <summary>
    /// MEF interface for showing the GitHub login dialog.
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Show the GitHub login dialog.
        /// </summary>
        void ShowLoginDialog();
    }
}