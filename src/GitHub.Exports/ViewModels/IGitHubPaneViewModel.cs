using System.Collections.ObjectModel;
using System.Windows.Input;
using GitHub.UI;

namespace GitHub.ViewModels
{
    public interface IGitHubPaneViewModel : IViewModel
    {
        string ActiveRepoName { get; }
        IView Control { get; }
        string Message { get; }
        MessageType MessageType { get; }
    }
}