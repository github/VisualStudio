using GitHub.UI;

namespace GitHub.VisualStudio.TeamExplorer
{
    public interface IGitHubInvitationSection
    {
        string Name { get; }
        string Description { get; }
        bool ShowLogin { get; }
        bool ShowSignup { get;  }
        bool ShowGetStarted { get; }
        void Connect();
        void SignUp();
    }
}
