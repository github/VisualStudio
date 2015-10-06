using System.Windows.Input;

namespace GitHub.ViewModels
{
    public interface IGitHubPaneViewModel : IViewModel
    {
        string ActiveRepoName { get; }
    }
}