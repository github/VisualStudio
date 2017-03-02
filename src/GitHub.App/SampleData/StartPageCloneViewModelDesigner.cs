using System;
using System.Reactive;
using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class StartPageCloneViewModelDesigner : DialogViewModelBase, IBaseCloneViewModel
    {
        public string BaseRepositoryPath { get; set; }
        public ReactivePropertyValidator<string> BaseRepositoryPathValidator { get; }
        public ICommand BrowseForDirectory { get; }
        public IReactiveCommand<object> CloneCommand { get; }
        public IRepositoryModel SelectedRepository { get; set; }
    }
}
