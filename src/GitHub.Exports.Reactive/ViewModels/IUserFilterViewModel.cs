using System.Collections.Generic;
using System.ComponentModel;

namespace GitHub.ViewModels
{
    public interface IUserFilterViewModel : IViewModel
    {
        string Filter { get; set; }
        string Header { get; }
        IActorViewModel Selected { get; set; }
        IReadOnlyList<IActorViewModel> Users { get; }
        ICollectionView UsersView { get; }
    }
}