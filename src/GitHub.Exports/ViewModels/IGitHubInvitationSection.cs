using GitHub.UI;

namespace GitHub.VisualStudio.TeamExplorer
{
    public interface IGitHubInvitationSection
    {
        string Name { get; }
        string Description { get; }
        bool CanSignUp { get; }
        bool CanConnect { get;  }
        void Connect();
        void SignUp();
        void Login();
    }
}
