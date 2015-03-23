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
        readonly string clonePath;

        public IReactiveCommand<Unit> CloneCommand { get; private set; }

        IReactiveList<IRepositoryModel> repositories;
        public IReactiveList<IRepositoryModel> Repositories
        {
            get { return repositories; }
            private set { this.RaiseAndSetIfChanged(ref repositories, value); }
        }

        IReactiveDerivedList<IRepositoryModel> _filteredRepositories;
        public IReactiveDerivedList<IRepositoryModel> FilteredRepositories
        {
            get { return _filteredRepositories; }
            private set { this.RaiseAndSetIfChanged(ref _filteredRepositories, value); }
        }

        IRepositoryModel _selectedRepository;
        [AllowNull]
        public IRepositoryModel SelectedRepository
        {
            [return: AllowNull]
            get { return _selectedRepository; }
            set { this.RaiseAndSetIfChanged(ref _selectedRepository, value); }
        }

        readonly ObservableAsPropertyHelper<bool> filterTextIsEnabled;
        public bool FilterTextIsEnabled { get { return filterTextIsEnabled.Value; } }

        string filterText;
        [AllowNull]
        public string FilterText
        {
            [return:AllowNull]
            get { return filterText; }
            set { this.RaiseAndSetIfChanged(ref filterText, value); }
        }

        [ImportingConstructor]
        public RepositoryCloneViewModel(IRepositoryCloneService cloneService, IRepositoryHosts hosts)
        {
            this.cloneService = cloneService;

            // TODO: Pick a better location to clone this.
            this.clonePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GitHub");

            // TODO: How do I know which host this dialog is associated with?
            // For now, I'll assume GitHub Host.
            Repositories = new ReactiveList<IRepositoryModel>();
            hosts.GitHubHost.Cache.GetUser()
                .Catch<User, KeyNotFoundException>(_ => Observable.Empty<User>())
                .SelectMany(user => hosts.GitHubHost.ApiClient.GetUserRepositories(user.Id))
                .SelectMany(repo => repo)
                // TODO: hasLocalClone is a bit hacky right now
                .Select(repo => new RepositoryModel(repo) { HasLocalClone = Directory.Exists(Path.Combine(clonePath, repo.Name)) })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Repositories.Add);

            filterTextIsEnabled = this.WhenAny(x => x.Repositories.Count, x => x.Value > 0)
                .ToProperty(this, x => x.FilterTextIsEnabled);

            var filterResetSignal = this.WhenAny(x => x.FilterText, x => x.Value)
                .DistinctUntilChanged(StringComparer.OrdinalIgnoreCase)
                .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler);

            FilteredRepositories = Repositories.CreateDerivedCollection(
                x => x,
                filter: FilterRepository,
                signalReset: filterResetSignal
            );

            var canCloneSelected = this.WhenAny(x => x.SelectedRepository, x => x.Value)
                .Select(selected => selected == null
                    ? Observable.Return(false)
                    : selected.WhenAny(x => x.HasLocalClone, x => x.Value == false))
                .Switch();

            CloneCommand = ReactiveCommand.CreateAsyncObservable(
                canCloneSelected.StartWith(false),
                OnCloneRepository
            );

        }

        bool FilterRepository(IRepositoryModel repo)
        {
            if (string.IsNullOrWhiteSpace(FilterText))
                return true;

            // Not matching on NameWithOwner here since that's already been filtered on by the selected account
            return repo.Name.IndexOf(FilterText ?? "", StringComparison.OrdinalIgnoreCase) != -1;
        }

        IObservable<Unit> OnCloneRepository(object state)
        {
            return Observable.Start(() =>
            {
                var repository = SelectedRepository;
                string baseRepositoryDirectory = clonePath;
                Directory.CreateDirectory(baseRepositoryDirectory);
                return cloneService.CloneRepository(repository.CloneUrl, repository.Name, baseRepositoryDirectory);
            })
            .SelectMany(_ => _);
        }
    }
}
