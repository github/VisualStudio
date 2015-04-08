using GitHub.UI;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    public interface IGitHubInvitationSection
    {
        string Description { get; set; }
        void Connect();
        void SignUp();
    }
}
