using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog.Clone;
using ReactiveUI;

namespace GitHub.SampleData.Dialog.Clone
{
    public class RepositoryCloneViewModelDesigner : ViewModelBase, IRepositoryCloneViewModel
    {
        public RepositoryCloneViewModelDesigner()
        {
            GitHubTab = new SelectPageViewModelDesigner();
            EnterpriseTab = new SelectPageViewModelDesigner();
        }

        public string Path { get; set; }
        public string PathError { get; set; }
        public int SelectedTabIndex { get; set; }
        public string Title => null;
        public IObservable<object> Done => null;
        public IRepositorySelectViewModel GitHubTab { get; }
        public IRepositorySelectViewModel EnterpriseTab { get; }
        public IRepositoryUrlViewModel UrlTab { get; }
        public ReactiveCommand<CloneDialogResult> Clone { get; }

        public Task InitializeAsync(IConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}
