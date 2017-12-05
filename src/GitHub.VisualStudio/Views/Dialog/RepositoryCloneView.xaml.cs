using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels.Dialog;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.Dialog
{
    public class GenericRepositoryCloneView : ViewBase<IRepositoryCloneViewModel, RepositoryCloneView>
    {}

    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportViewFor(typeof(IRepositoryCloneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryCloneView : GenericRepositoryCloneView
    {
        readonly Dictionary<string, RepositoryGroup> groups = new Dictionary<string, RepositoryGroup>();

        static readonly DependencyPropertyKey RepositoriesViewPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(RepositoriesView),
                typeof(ICollectionView),
                typeof(RepositoryCloneView),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RepositoriesViewProperty = RepositoriesViewPropertyKey.DependencyProperty;

        public RepositoryCloneView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                //d(repositoryList.Events().MouseDoubleClick.InvokeCommand(this, x => x.ViewModel.CloneCommand));
            });

            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };

            this.WhenAnyValue(x => x.ViewModel.Repositories, CreateRepositoryListCollectionView).Subscribe(x => RepositoriesView = x);
        }

        public ICollectionView RepositoriesView
        {
            get { return (ICollectionView)GetValue(RepositoriesViewProperty); }
            private set { SetValue(RepositoriesViewPropertyKey, value); }
        }

        ListCollectionView CreateRepositoryListCollectionView(IEnumerable<IRemoteRepositoryModel> repositories)
        {
            if (repositories == null)
                return null;

            var view = new ListCollectionView((IList)repositories);
            view.GroupDescriptions.Add(new RepositoryGroupDescription(this));
            return view;
        }

        class RepositoryGroupDescription : GroupDescription
        {
            readonly RepositoryCloneView owner;

            public RepositoryGroupDescription(RepositoryCloneView owner)
            {
                Guard.ArgumentNotNull(owner, nameof(owner));

                this.owner = owner;
            }

            public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
            {
                var repo = item as IRemoteRepositoryModel;
                var name = repo?.Owner ?? string.Empty;
                RepositoryGroup group;

                if (!owner.groups.TryGetValue(name, out group))
                {
                    group = new RepositoryGroup(name, owner.groups.Count == 0);

                    if (owner.groups.Count == 1)
                        owner.groups.Values.First().IsExpanded = false;
                    owner.groups.Add(name, group);
                }

                return group;
            }
        }

        class RepositoryGroup : ReactiveObject
        {
            bool isExpanded;

            public RepositoryGroup(string header, bool isExpanded)
            {
                Guard.ArgumentNotEmptyString(header, nameof(header));

                Header = header;
                this.isExpanded = isExpanded;
            }

            public string Header { get; }

            public bool IsExpanded
            {
                get { return isExpanded; }
                set { this.RaiseAndSetIfChanged(ref isExpanded, value); }
            }
        }
    }
}
