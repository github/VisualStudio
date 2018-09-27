using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.Documents
{
    public interface IIssueDetailViewModel : IPaneViewModel
    {
        ICommentViewModel Body { get; }
        ObservableCollection<ICommentViewModel> Comments { get; }
        int Number { get; }
        string Title { get; set; }

        Task InitializeAsync(
            IConnection connection,
            string owner,
            string repo,
            int number);
    }
}