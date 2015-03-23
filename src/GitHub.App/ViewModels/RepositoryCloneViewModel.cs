using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using NullGuard;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    public class RepositoryCloneViewModel : ReactiveObject, IRepositoryCloneViewModel
    {
        public string Title { get { return "Clone a GitHub Repository"; } } // TODO: this needs to be contextual

        readonly IRepositoryCloneService cloneService;

        public IReactiveCommand<Unit> CloneCommand { get; private set; }

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
        public RepositoryCloneViewModel(IRepositoryCloneService cloneService, IRepositoryHosts hosts)
        {
            this.cloneService = cloneService;

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

            CloneCommand = ReactiveCommand.CreateAsyncObservable(OnCloneRepository);
        }

        IObservable<Unit> OnCloneRepository(object state)
        {
            return Observable.Start(() =>
            {
                var repository = SelectedRepository;

                // TODO: Pick a better location to clone this.
                string baseRepositoryDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GitHub");
                Directory.CreateDirectory(baseRepositoryDirectory);
                return cloneService.CloneRepository(repository.CloneUrl, repository.Name, baseRepositoryDirectory);
            })
            .SelectMany(_ => _);
        }
    }
}
