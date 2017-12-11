using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class RepositoryRecloneViewModelDesigner : ViewModelBase, IRepositoryRecloneViewModel
    {
        public string Title { get; set; }
        public string BaseRepositoryPath { get; set; }
        public ReactivePropertyValidator<string> BaseRepositoryPathValidator { get; }
        public ICommand BrowseForDirectory { get; }
        public IReactiveCommand<object> CloneCommand { get; }
        public IRepositoryModel SelectedRepository { get; set; }
        public IObservable<object> Done { get; }

        public Task InitializeAsync(IConnection connection) => Task.CompletedTask;
    }
}
