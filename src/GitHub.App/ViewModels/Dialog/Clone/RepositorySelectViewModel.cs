using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels.Dialog.Clone
{
    [Export(typeof(IRepositorySelectViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositorySelectViewModel : ViewModelBase, IRepositorySelectViewModel
    {
        static readonly ILogger log = LogManager.ForContext<RepositorySelectViewModel>();
        readonly IRepositoryCloneService service;
        readonly IGitHubContextService gitHubContextService;
        IConnection connection;
        Exception error;
        string filter;
        bool isEnabled;
        bool isLoading;
        IReadOnlyList<IRepositoryItemViewModel> items;
        ICollectionView itemsView;
        ObservableAsPropertyHelper<RepositoryModel> repository;
        IRepositoryItemViewModel selectedItem;

        [ImportingConstructor]
        public RepositorySelectViewModel(IRepositoryCloneService service, IGitHubContextService gitHubContextService)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(service, nameof(gitHubContextService));

            this.service = service;
            this.gitHubContextService = gitHubContextService;

            var selectedRepository = this.WhenAnyValue(x => x.SelectedItem)
                .Select(CreateRepository);

            var filterRepository = this.WhenAnyValue(x => x.Filter)
                .Select(f => gitHubContextService.FindContextFromUrl(f))
                .Select(CreateRepository);

            repository = selectedRepository
                .Merge(filterRepository)
                .ToProperty(this, x => x.Repository);

            this.WhenAnyValue(x => x.Filter).Subscribe(_ => ItemsView?.Refresh());
        }

        public Exception Error
        {
            get => error;
            private set => this.RaiseAndSetIfChanged(ref error, value);
        }

        public string Filter
        {
            get => filter;
            set => this.RaiseAndSetIfChanged(ref filter, value);
        }

        public bool IsEnabled
        {
            get => isEnabled;
            private set => this.RaiseAndSetIfChanged(ref isEnabled, value);
        }

        public bool IsLoading
        {
            get => isLoading;
            private set => this.RaiseAndSetIfChanged(ref isLoading, value);
        }

        public IReadOnlyList<IRepositoryItemViewModel> Items
        {
            get => items;
            private set => this.RaiseAndSetIfChanged(ref items, value);
        }

        public ICollectionView ItemsView
        {
            get => itemsView;
            private set => this.RaiseAndSetIfChanged(ref itemsView, value);
        }

        public IRepositoryItemViewModel SelectedItem
        {
            get => selectedItem;
            set => this.RaiseAndSetIfChanged(ref selectedItem, value);
        }

        public RepositoryModel Repository => repository.Value;

        public void Initialize(IConnection connection)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));

            this.connection = connection;
            IsEnabled = true;
        }

        public async Task Activate()
        {
            await this.LoadItems(true);
        }

        static string GroupName(KeyValuePair<string, IReadOnlyList<RepositoryListItemModel>> group, int max)
        {
            var name = group.Key;
            if (group.Value.Count == max)
            {
                name += $" ({string.Format(CultureInfo.InvariantCulture, Resources.MostRecentlyPushed, max)})";
            }

            return name;
        }

        async Task LoadItems(bool refresh)
        {
            if (connection == null || IsLoading) return;

            Error = null;
            IsLoading = true;

            try
            {
                if (refresh)
                {
                    Items = new List<IRepositoryItemViewModel>();
                    ItemsView = CollectionViewSource.GetDefaultView(Items);
                }

                var results = await log.TimeAsync(nameof(service.ReadViewerRepositories),
                    () => service.ReadViewerRepositories(connection.HostAddress, refresh));

                var yourRepositories = results.Repositories
                    .Where(r => r.Owner == results.Owner)
                    .Select(x => new RepositoryItemViewModel(x, Resources.RepositorySelectYourRepositories));
                var collaboratorRepositories = results.Repositories
                    .Where(r => r.Owner != results.Owner)
                    .OrderBy(r => r.Owner)
                    .Select(x => new RepositoryItemViewModel(x, Resources.RepositorySelectCollaboratorRepositories));
                var repositoriesContributedTo = results.ContributedToRepositories
                    .Select(x => new RepositoryItemViewModel(x, Resources.RepositorySelectContributedRepositories));
                var orgRepositories = results.Organizations
                    .OrderBy(x => x.Key)
                    .SelectMany(x => x.Value.Select(y => new RepositoryItemViewModel(y, GroupName(x, 100))));
                Items = yourRepositories
                    .Concat(collaboratorRepositories)
                    .Concat(repositoriesContributedTo)
                    .Concat(orgRepositories)
                    .ToList();
                log.Information("Read {Total} viewer repositories", Items.Count);
                ItemsView = CollectionViewSource.GetDefaultView(Items);
                ItemsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(RepositoryItemViewModel.Group)));
                ItemsView.Filter = FilterItem;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error reading repository list from {Address}", connection.HostAddress);

                if (ex is AggregateException aggregate)
                {
                    ex = aggregate.InnerExceptions[0];
                }

                Error = ex;
            }
            finally
            {
                IsLoading = false;
            }
        }

        bool FilterItem(object obj)
        {
            var trimedFilter = Filter?.Trim();
            if (obj is IRepositoryItemViewModel item && !string.IsNullOrEmpty(trimedFilter))
            {
                if (new UriString(trimedFilter).IsHypertextTransferProtocol)
                {
                    var urlString = item.Url.ToString();
                    var urlStringWithGit = urlString + ".git";
                    var urlStringWithSlash = urlString + "/";
                    return
                        urlString.Contains(trimedFilter, StringComparison.OrdinalIgnoreCase) ||
                        urlStringWithGit.Contains(trimedFilter, StringComparison.OrdinalIgnoreCase) ||
                        urlStringWithSlash.Contains(trimedFilter, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    return
                        item.Caption.Contains(trimedFilter, StringComparison.CurrentCultureIgnoreCase);
                }
            }

            return true;
        }

        RepositoryModel CreateRepository(IRepositoryItemViewModel item)
        {
            return item != null ?
                new RepositoryModel(item.Name, UriString.ToUriString(item.Url)) :
                null;
        }

        RepositoryModel CreateRepository(GitHubContext context)
        {
            switch (context?.LinkType)
            {
                case LinkType.Repository:
                case LinkType.Blob:
                    return new RepositoryModel(context.RepositoryName, context.Url);
            }

            return null;
        }
    }
}
