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
using GitHub.Services;
using System.Linq;

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
        readonly Dictionary<string, RepositoryGroup> groups = new Dictionary<string, RepositoryGroup>();

        public RepositoryCloneControl()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.Repositories, v => v.repositoryList.ItemsSource, CreateRepositoryListCollectionView));
                d(repositoryList.Events().MouseDoubleClick.InvokeCommand(this, x => x.ViewModel.CloneCommand));
                d(ViewModel.CloneCommand.Subscribe(_ => NotifyDone()));
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }

        ListCollectionView CreateRepositoryListCollectionView(IEnumerable<IRepositoryModel> repositories)
        {
            var view = new ListCollectionView((IList)repositories);
            Debug.Assert(view.GroupDescriptions != null, "view.GroupDescriptions is null");
            view.GroupDescriptions.Add(new RepositoryGroupDescription(this));
            return view;
        }

        class RepositoryGroupDescription : GroupDescription
        {
            readonly RepositoryCloneControl owner;

            public RepositoryGroupDescription(RepositoryCloneControl owner)
            {
                this.owner = owner;
            }

            public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
            {
                var repo = item as IRepositoryModel;
                Debug.Assert(repo.Owner != null, "Repository owner cannot be null, did something get changed in the RepositoryModel ctor?");
                var name = repo.Owner.Login;
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
