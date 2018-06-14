using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IIssueListViewModelBase : ISearchablePageViewModel
    {
        IReadOnlyList<IViewModel> Items { get; }

        ICollectionView ItemsView { get; }

        ILocalRepositoryModel LocalRepository { get; }

        string SelectedState { get; set; }

        IReadOnlyList<string> States { get; }

        ReactiveCommand<Unit> OpenItem { get; }

        Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);
    }
}
