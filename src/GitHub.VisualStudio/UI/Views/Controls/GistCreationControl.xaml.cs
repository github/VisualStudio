using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Threading;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using Microsoft.VisualStudio.Shell;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericGistCreationControl : ViewBase<IGistCreationViewModel, GistCreationControl>
    { }

    [ExportView(ViewType=UIViewType.Gist)]
    [PartCreationPolicy(CreationPolicy.NonShared)] 
    public partial class GistCreationControl : GenericGistCreationControl
    {
        [ImportingConstructor]
        public GistCreationControl(
            INotificationDispatcher notifications,
            IGitHubServiceProvider serviceProvider)
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                errorMessage.Visibility = Visibility.Collapsed;

                d(this.Bind(ViewModel, vm => vm.Description, v => v.descriptionTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.FileName, v => v.fileNameTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.IsPrivate, v => v.makePrivate.IsChecked));
                d(this.BindCommand(ViewModel, vm => vm.CreateGist, v => v.createGistButton));

                d(this.Bind(ViewModel, vm => vm.Account, v => v.accountStackPanel.DataContext));

                ViewModel.CreateGist
                    .Where(x => x != null)
                    .Subscribe(gist =>
                    {
                        var browser = serviceProvider.TryGetService<IVisualStudioBrowser>();
                        browser?.OpenUrl(new Uri(gist.HtmlUrl));

                        var ns = serviceProvider.TryGetService<IStatusBarNotificationService>();
                        ns?.ShowMessage(UI.Resources.gistCreatedMessage);
                    });

                d(notifications.Listen()
                    .Where(n => n.Type == Notification.NotificationType.Error)
                    .ObserveOnDispatcher(DispatcherPriority.Normal)
                    .Subscribe(n =>
                    {
                        errorMessage.Visibility = Visibility.Visible;
                        errorMessageText.Text = n.Message;
                    }));
            });
        }
    }
}
