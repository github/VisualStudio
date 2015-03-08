using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    public class CloneRepoViewModel : ICloneRepoViewModel
    {
        public string Title { get { return "Clone a GitHub Repository"; } } // TODO: this needs to be contextual

        IReactiveCommand<object> cancelCommand = ReactiveCommand.Create();
        IReactiveCommand<object> okCommand = ReactiveCommand.Create();

        public ICommand CancelCommand { get { return cancelCommand; } }
        public IObservable<object> Cancelling { get { return cancelCommand; } }
        public ICommand OkCommand { get { return okCommand; } }

        public ICollection<IRepositoryModel> Repositories
        {
            get;
            private set;
        }

        [ImportingConstructor]
        public CloneRepoViewModel(IRepositoryHosts hosts)
        {
            Repositories = new List<IRepositoryModel>
            {
                new RepositoryModel {Owner = "haacked", Name = "encourage" },
                new RepositoryModel {Owner = "haacked", Name = "seegit" },
                new RepositoryModel {Owner = "haacked", Name = "routemagic" },
                new RepositoryModel {Owner = "haacked", Name = "subtext" },
                new RepositoryModel {Owner = "haacked", Name = "haacked.com" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.net" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.rb" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.objc" },
                new RepositoryModel {Owner = "github", Name = "github" },
                new RepositoryModel {Owner = "github", Name = "windows" },
                new RepositoryModel {Owner = "github", Name = "mac" },
                new RepositoryModel {Owner = "github", Name = "desktop" },
            };
        }
    }
}
