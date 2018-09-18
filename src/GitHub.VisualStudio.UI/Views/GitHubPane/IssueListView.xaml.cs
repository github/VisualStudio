using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI.Helpers;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [ExportViewFor(typeof(IIssueListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class IssueListView : UserControl
    {
        IDisposable subscription;

        [ImportingConstructor]
        public IssueListView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                var vm = DataContext as IIssueListViewModel;
                subscription?.Dispose();
                subscription = null;

                if (vm != null)
                {
                    subscription = new CompositeDisposable(
                        vm.AuthorFilter.WhenAnyValue(x => x.Selected)
                            .Skip(1)
                            .Subscribe(_ => authorFilterDropDown.IsOpen = false),
                        vm.OpenItemInBrowser.Subscribe(x => OpenInBrowser((IIssueListItemViewModel)x)));
                }
            };

            Unloaded += (s, e) =>
            {
                subscription?.Dispose();
                subscription = null;
            };
        }

        [Import]
        IVisualStudioBrowser VisualStudioBrowser { get; set; }

        void OpenInBrowser(IIssueListItemViewModel item)
        {
            var vm = DataContext as IIssueListViewModel;

            if (vm != null)
            {
                var uri = vm.RemoteRepository.CloneUrl.ToRepositoryUrl().Append("issue/" + item.Number);
                VisualStudioBrowser.OpenUrl(uri);
            }
        }

        void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            var listBox = (ListBox)sender;

            if (listBox.SelectedItem != null && e.Key == Key.Enter)
            {
                var vm = DataContext as IIssueListViewModel;
                var pr = (IIssueListItemViewModel)listBox.SelectedItem;
                vm.OpenItem.Execute(pr);
            }
        }

        void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = sender as ListBoxItem;
            var pr = control?.DataContext as IIssueListItemViewModel;
            var vm = DataContext as IIssueListViewModel;

            if (pr != null && vm != null)
            {
                vm.OpenItem.Execute(pr);
            }
        }

        void authorFilterDropDown_PopupOpened(object sender, EventArgs e)
        {
            authorFilter.FocusSearchBox();
        }

        void ListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ApplyContextMenuBinding<ListBoxItem>(sender, e);
        }

        void ApplyContextMenuBinding<TItem>(object sender, ContextMenuEventArgs e) where TItem : Control
        {
            var container = (Control)sender;
            var item = GetVisual(e.OriginalSource)?.GetSelfAndVisualAncestors().OfType<TItem>().FirstOrDefault();

            e.Handled = true;

            if (item?.DataContext is IIssueListItemViewModel listItem)
            {
                container.ContextMenu.DataContext = this.DataContext;

                foreach (var menuItem in container.ContextMenu.Items.OfType<MenuItem>())
                {
                    menuItem.CommandParameter = listItem;
                }

                e.Handled = false;
            }
        }

        Visual GetVisual(object element)
        {
            if (element is Visual v)
            {
                return v;
            }
            else if (element is TextElement e)
            {
                return e.Parent as Visual;
            }
            else
            {
                return null;
            }
        }
    }
}
