using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Documents;

namespace GitHub.SampleData
{
    public class IssueDetailViewModelDesigner : ViewModelBase, IIssueDetailViewModel
    {
        public ICommentViewModel Body { get; set; }
        public ObservableCollection<ICommentViewModel> Comments { get; }
        public int Number { get; set; }
        public string Title { get; set; }

        public Task InitializeAsync(IConnection connection, string owner, string repo, int number) => Task.CompletedTask;
        public Task InitializeAsync(IServiceProvider paneServiceProvider) => Task.CompletedTask;
    }
}
