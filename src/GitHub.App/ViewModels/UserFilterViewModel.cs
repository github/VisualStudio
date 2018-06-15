using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public class UserFilterViewModel : ViewModelBase, IUserFilterViewModel
    {
        readonly VirtualizingList<IActorViewModel> users;
        readonly LoadPageDelegate load;
        ObservableAsPropertyHelper<string> header;
        string filter;
        IActorViewModel selected;

        public delegate Task<Page<ActorModel>> LoadPageDelegate(string after);

        public UserFilterViewModel(string header, LoadPageDelegate load)
        {
            this.load = load;
            users = new VirtualizingList<IActorViewModel>(new UserSource(this), null);
            UsersView = new VirtualizingListCollectionView<IActorViewModel>(users);
            UsersView.Filter = FilterUsers;
            this.WhenAnyValue(x => x.Filter).Subscribe(_ => UsersView.Refresh());
            this.header = this.WhenAnyValue(x => x.Selected, x => x?.Login ?? header)
                .ToProperty(this, x => x.Header);
        }

        public IReadOnlyList<IActorViewModel> Users { get; }
        public ICollectionView UsersView { get; }
        public string Header => header.Value;

        public string Filter
        {
            get { return filter; }
            set { this.RaiseAndSetIfChanged(ref filter, value); }
        }

        public IActorViewModel Selected
        {
            get { return selected; }
            set { this.RaiseAndSetIfChanged(ref selected, value); }
        }

        bool FilterUsers(object obj)
        {
            if (Filter != null)
            {
                var user = obj as IActorViewModel;
                return user?.Login.IndexOf(Filter, StringComparison.CurrentCultureIgnoreCase) != -1;
            }

            return true;
        }

        class UserSource : SequentialListSource<ActorModel, IActorViewModel>
        {
            readonly UserFilterViewModel owner;

            public UserSource(UserFilterViewModel owner)
            {
                this.owner = owner;
            }

            protected override IActorViewModel CreateViewModel(ActorModel model)
            {
                var result = new ActorViewModel(model);
                return result;
            }

            protected override Task<Page<ActorModel>> LoadPage(string after)
            {
                return owner.load(after);
            }
        }
    }
}