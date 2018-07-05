using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using GitHub.ViewModels;

namespace GitHub.SampleData
{
    public class UserFilterViewModelDesigner : ViewModelBase, IUserFilterViewModel
    {
        public UserFilterViewModelDesigner()
        {
            Users = new[]
            {
                new ActorViewModelDesigner("grokys"),
                new ActorViewModelDesigner("jcansdale"),
                new ActorViewModelDesigner("meaghanlewis"),
                new ActorViewModelDesigner("sguthals"),
                new ActorViewModelDesigner("shana"),
                new ActorViewModelDesigner("StanleyGoldman"),
            };

            UsersView = CollectionViewSource.GetDefaultView(Users);
        }

        public string Filter { get; set; }
        public IActorViewModel Selected { get; set; }
        public IReadOnlyList<IActorViewModel> Users { get; }
        public ICollectionView UsersView { get; }
    }
}
