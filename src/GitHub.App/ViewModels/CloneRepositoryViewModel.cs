using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Models;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    public class CloneRepositoryViewModel : ICloneRepositoryViewModel
    {
        public string Title { get { return "Clone a GitHub Repository"; } } // TODO: this needs to be contextual

        IReactiveCommand<object> cloneCommand = ReactiveCommand.Create();

        public ICommand CloneCommand { get { return cloneCommand; } }

        public ICollection<IRepositoryModel> Repositories
        {
            get;
            private set;
        }

        [ImportingConstructor]
        public CloneRepositoryViewModel(IRepositoryHosts hosts)
        {
            // TODO: How do I know which host this dialog is associated with?
            // For now, I'll assume GitHub Host.
            Repositories = new ReactiveList<IRepositoryModel>();
            hosts.GitHubHost.Cache.GetUser()
                .Catch<User, KeyNotFoundException>(_ => Observable.Empty<User>())
                .SelectMany(user => hosts.GitHubHost.ApiClient.GetUserRepositories(user.Id))
                .SelectMany(repo => repo)
                .Select(repo => new RepositoryModel { Owner = repo.Owner.Login, Name = repo.Name })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Repositories.Add);
        }
    }
}
