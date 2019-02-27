using System.Collections.Generic;
using System.ComponentModel;

namespace GitHub.ViewModels
{
    public interface IUserFilterViewModel : IViewModel
    {
        string Filter { get; set; }
        IActorViewModel Selected { get; set; }
        IReadOnlyList<IActorViewModel> Users { get; }
        ICollectionView UsersView { get; }
    }
}