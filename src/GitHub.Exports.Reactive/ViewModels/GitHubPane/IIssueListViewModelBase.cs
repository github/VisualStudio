using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IIssueListViewModelBase : ISearchablePageViewModel
    {
        IReadOnlyList<IViewModel> Items { get; }

        ICollectionView ItemsView { get; }

        ILocalRepositoryModel LocalRepository { get; }

        Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);
    }
}
