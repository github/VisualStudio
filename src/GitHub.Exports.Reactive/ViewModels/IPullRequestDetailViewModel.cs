using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IPullRequestDetailViewModel : IViewModel
    {
        int Number { get; }
        IAccount Author { get; }
        string Body { get; }

        ReactiveCommand<object> OpenOnGitHub { get; }
    }
}
