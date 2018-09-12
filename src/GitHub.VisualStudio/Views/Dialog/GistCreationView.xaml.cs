using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Threading;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels.Dialog;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.Dialog
{
    public class GenericGistCreationView : ViewBase<IGistCreationViewModel, GistCreationView>
    { }

    public class FooBar { }

    [ExportViewFor(typeof(IGistCreationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)] 
    public partial class GistCreationView : GenericGistCreationView
    {
        [ImportingConstructor]
        public GistCreationView(
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
                        ns?.ShowMessage(GitHub.Resources.gistCreatedMessage);
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
