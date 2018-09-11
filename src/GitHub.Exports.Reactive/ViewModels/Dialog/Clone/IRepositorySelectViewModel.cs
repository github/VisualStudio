using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.Dialog.Clone
{
    public interface IRepositorySelectViewModel : IRepositoryCloneTabViewModel
    {
        Exception Error { get; }
        string Filter { get; set; }
        bool IsLoading { get; }
        IReadOnlyList<IRepositoryItemViewModel> Items { get; }
        ICollectionView ItemsView { get; }
        IRepositoryItemViewModel SelectedItem { get; set; }

        void Initialize(IConnection connection);
    }
}
