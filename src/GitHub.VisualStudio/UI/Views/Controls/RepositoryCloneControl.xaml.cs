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

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Clone)]
    public partial class RepositoryCloneControl : IViewFor<IRepositoryCloneViewModel>, IView
    {
        public RepositoryCloneControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IRepositoryCloneViewModel;

            close = new Subject<object>();

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.Repositories, v => v.repositoryList.ItemsSource, CreateRepositoryListCollectionView));
                d(this.Bind(ViewModel, vm => vm.SelectedRepository, v => v.repositoryList.SelectedItem));
                d(this.BindCommand(ViewModel, vm => vm.CloneCommand, v => v.cloneButton));
                d(ViewModel.CloneCommand.Subscribe(_ => { close.OnNext(null); close.OnCompleted(); }));
            });
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

        static ListCollectionView CreateRepositoryListCollectionView(ICollection<IRepositoryModel> repositories)
        {
            var view = new ListCollectionView((IList)repositories);
            view.GroupDescriptions.Add(new RepositoryGroupDescription());
            return view;
        }

        class RepositoryGroupDescription : GroupDescription
        {
            public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
            {
                return ((IRepositoryModel)item).Owner;
            }

            public override bool NamesMatch(object groupName, object itemName)
            {
                return string.Equals((string)groupName, (string)itemName);
            }
        }

        Subject<object> close;
        public IObservable<object> Done { get { return close; } }
    }
}
