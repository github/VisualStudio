using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using GitHub.Exports;
using GitHub.Models;
using GitHub.UI;
using GitHub.UI.Helpers;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using System;
using GitHub.Extensions.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Windows.Input;
using GitHub.Extensions;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Clone)]
    public partial class RepositoryCloneControl : IViewFor<IRepositoryCloneViewModel>, IView, IDisposable
    {
        readonly Subject<object> close;

        public RepositoryCloneControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");

            InitializeComponent();

            close = new Subject<object>();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IRepositoryCloneViewModel;

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.FilteredRepositories, v => v.repositoryList.ItemsSource, CreateRepositoryListCollectionView));
                d(this.Bind(ViewModel, vm => vm.SelectedRepository, v => v.repositoryList.SelectedItem));
                d(this.BindCommand(ViewModel, vm => vm.CloneCommand, v => v.cloneButton));
                d(this.OneWayBind(ViewModel, vm => vm.FilterTextIsEnabled, v => v.filterText.IsEnabled));
                d(this.Bind(ViewModel, vm => vm.FilterText, v => v.filterText.Text));
                ViewModel.CloneCommand.Subscribe(_ => { close.OnNext(null); close.OnCompleted(); });
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
           "ViewModel", typeof(IRepositoryCloneViewModel), typeof(RepositoryCloneControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IRepositoryCloneViewModel)value; }
        }

        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IRepositoryCloneViewModel)value; }
        }

        public IRepositoryCloneViewModel ViewModel
        {
            [return: AllowNull]
            get { return (IRepositoryCloneViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        static ListCollectionView CreateRepositoryListCollectionView(IEnumerable<IRepositoryModel> repositories)
        {
            var view = new ListCollectionView((IList)repositories);
            view.GroupDescriptions.Add(new RepositoryGroupDescription());
            return view;
        }

        class RepositoryGroupDescription : GroupDescription
        {
            public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
            {
                return ((IRepositoryModel)item).Owner.Login;
            }

            public override bool NamesMatch(object groupName, object itemName)
            {
                return string.Equals((string)groupName, (string)itemName);
            }
        }

        public IObservable<object> Done { get { return close; } }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    close.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
