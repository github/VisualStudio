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
using ReactiveUI;
using System.ComponentModel.Composition;
using GitHub.Services;
using System.Linq;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericRepositoryCloneControl : ViewBase<IRepositoryCloneViewModel, RepositoryCloneControl>
    {}

    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Clone)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryCloneControl : GenericRepositoryCloneControl
    {
        readonly Dictionary<string, RepositoryGroup> groups = new Dictionary<string, RepositoryGroup>();

        static readonly DependencyPropertyKey RepositoriesViewPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(RepositoriesView),
                typeof(ICollectionView),
                typeof(RepositoryCloneControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RepositoriesViewProperty = RepositoriesViewPropertyKey.DependencyProperty;

        public RepositoryCloneControl()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(repositoryList.Events().MouseDoubleClick.InvokeCommand(this, x => x.ViewModel.CloneCommand));
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
            readonly RepositoryCloneControl owner;

            public RepositoryGroupDescription(RepositoryCloneControl owner)
            {
                Guard.ArgumentNotNull(owner, nameof(owner));

                this.owner = owner;
            }

            public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
            {
                Guard.ArgumentNotNull(item, nameof(item));
                Guard.ArgumentNotNull(culture, nameof(culture));

                var repo = item as IRemoteRepositoryModel;
                var name = repo.Owner;
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
