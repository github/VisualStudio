using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog.Clone;

namespace GitHub.SampleData.Dialog.Clone
{
    public class SelectPageViewModelDesigner : ViewModelBase, IRepositorySelectViewModel
    {
        public SelectPageViewModelDesigner()
        {
            var items = new[]
            {
                    new RepositoryListItemModel { Name = "encourage", Owner = "haacked" },
                    new RepositoryListItemModel { Name = "haacked.com", Owner = "haacked", IsFork = true },
                    new RepositoryListItemModel { Name = "octokit.net", Owner = "octokit" },
                    new RepositoryListItemModel { Name = "octokit.rb", Owner = "octokit" },
                    new RepositoryListItemModel { Name = "octokit.objc", Owner = "octokit" },
                    new RepositoryListItemModel { Name = "windows", Owner = "github" },
                    new RepositoryListItemModel { Name = "mac", Owner = "github", IsPrivate = true },
                    new RepositoryListItemModel { Name = "github", Owner = "github", IsPrivate = true }
                };

            Items = items.Select(x => new RepositoryItemViewModel(x, x.Owner)).ToList();
            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(RepositoryItemViewModel.Group)));
        }

        public Exception Error { get; set; }
        public string Filter { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsLoading { get; set; }
        public IReadOnlyList<IRepositoryItemViewModel> Items { get; }
        public ICollectionView ItemsView { get; }
        public IRepositoryItemViewModel SelectedItem { get; set; }
        public RepositoryModel Repository { get; }

        public void Initialize(IConnection connection)
        {
        }

        public Task Activate()
        {
            return Task.CompletedTask;
        }
    }
}
