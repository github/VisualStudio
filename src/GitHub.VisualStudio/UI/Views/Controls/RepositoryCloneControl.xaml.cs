using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Clone)]
    public partial class RepositoryCloneControl : ViewUserControl, IViewFor<IRepositoryCloneViewModel>, IView
    {
        public RepositoryCloneControl()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IRepositoryCloneViewModel;

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.IsLoading, v => v.loadingProgressBar.Visibility));

                d(this.OneWayBind(ViewModel, vm => vm.FilteredRepositories, v => v.repositoryList.ItemsSource, CreateRepositoryListCollectionView));
                d(this.Bind(ViewModel, vm => vm.SelectedRepository, v => v.repositoryList.SelectedItem));
                d(this.BindCommand(ViewModel, vm => vm.CloneCommand, v => v.cloneButton));
                d(this.OneWayBind(ViewModel, vm => vm.FilterTextIsEnabled, v => v.filterText.IsEnabled));
                d(this.Bind(ViewModel, vm => vm.FilterText, v => v.filterText.Text));
                d(repositoryList.Events().MouseDoubleClick.InvokeCommand(this, x => x.ViewModel.CloneCommand));
                d(ViewModel.LoadRepositoriesCommand.ExecuteAsync().Subscribe());
                ViewModel.CloneCommand.Subscribe(_ => NotifyDone());
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
    }
}
