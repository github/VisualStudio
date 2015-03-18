using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Models;
using Octokit;
using ReactiveUI;
using NullGuard;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using System.Diagnostics;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    public class RepositoryCloneViewModel : ReactiveObject, IRepositoryCloneViewModel
    {
        readonly IServiceProvider serviceProvider;

        public string Title { get { return "Clone a GitHub Repository"; } } // TODO: this needs to be contextual

        IReactiveCommand<object> cloneCommand;

        public ICommand CloneCommand { get { return cloneCommand; } }

        public ICollection<IRepositoryModel> Repositories
        {
            get;
            private set;
        }

        IRepositoryModel _selectedRepository;
        [AllowNull]
        public IRepositoryModel SelectedRepository
        {
            [return: AllowNull]
            get { return _selectedRepository; }
            set { this.RaiseAndSetIfChanged(ref _selectedRepository, value); }
        }

        [ImportingConstructor]
        public RepositoryCloneViewModel(IServiceProvider serviceProvider, IRepositoryHosts hosts)
        {
            this.serviceProvider = serviceProvider;

            // TODO: How do I know which host this dialog is associated with?
            // For now, I'll assume GitHub Host.
            Repositories = new ReactiveList<IRepositoryModel>();
            hosts.GitHubHost.Cache.GetUser()
                .Catch<User, KeyNotFoundException>(_ => Observable.Empty<User>())
                .SelectMany(user => hosts.GitHubHost.ApiClient.GetUserRepositories(user.Id))
                .SelectMany(repo => repo)
                .Select(repo => new RepositoryModel(repo))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Repositories.Add);

            cloneCommand = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                var repo = SelectedRepository;
                var uri = repo.CloneUrl;
                return Observable.Start<object>(() =>
                {
                    var gitExt = serviceProvider.GetService(typeof(IGitRepositoriesExt)) as IGitRepositoriesExt;
                    Debug.Assert(gitExt != null, "Could not get an instance of IGitRepositoriesExt");

                    // TODO: use VS default dir for projects
                    var tmp = System.IO.Path.GetTempFileName();
                    System.IO.File.Delete(tmp);
                    var tmpname = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(tmp), System.IO.Path.GetFileNameWithoutExtension(tmp));
                    System.IO.Directory.CreateDirectory(tmpname);
                    gitExt.Clone(uri.ToString(), tmpname, CloneOptions.RecurseSubmodule);
                    return null;
                });
            });
        }
    }
}
