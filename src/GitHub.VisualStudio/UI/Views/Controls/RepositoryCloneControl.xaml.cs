using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericRepositoryCloneControl : SimpleViewUserControl<IRepositoryCloneViewModel, RepositoryCloneControl>
    {}

    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Clone)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryCloneControl : GenericRepositoryCloneControl
    {
        public RepositoryCloneControl()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IRepositoryCloneViewModel;

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.IsLoading, v => v.loadingProgressBar.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.LoadingFailed, v => v.loadingFailedPanel.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.NoRepositoriesFound, v => v.noRepositoriesMessage.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.FilteredRepositories, v => v.repositoryList.ItemsSource, CreateRepositoryListCollectionView));
                d(this.Bind(ViewModel, vm => vm.SelectedRepository, v => v.repositoryList.SelectedItem));
                d(this.Bind(ViewModel, vm => vm.BaseRepositoryPath, v => v.clonePath.Text));
                d(this.OneWayBind(ViewModel, vm => vm.BaseRepositoryPathValidator, v => v.pathValidationMessage.ReactiveValidator));
                d(this.BindCommand(ViewModel, vm => vm.BrowseForDirectory, v => v.browsePathButton));
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

        static ListCollectionView CreateRepositoryListCollectionView(IEnumerable<IRepositoryModel> repositories)
        {
            var view = new ListCollectionView((IList)repositories);
            Debug.Assert(view.GroupDescriptions != null, "view.GroupDescriptions is null");
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
