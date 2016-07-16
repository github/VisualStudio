using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using System.Windows;
using ReactiveUI;
using GitHub.Services;
using System.Windows.Threading;
using System.Reactive.Linq;
using System;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericGitHubPaneView : SimpleViewUserControl<IGitHubPaneViewModel, GitHubPaneView>
    {
    }

    [ExportView(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public partial class GitHubPaneView : GenericGitHubPaneView
    {
        [ImportingConstructor]
        public GitHubPaneView(INotificationDispatcher notifications)
        {
            this.InitializeComponent();
            this.WhenActivated(d =>
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
                d(notifications.Listen()
                    .Where(n => n.Type == Notification.NotificationType.Error)
                    .ObserveOnDispatcher(DispatcherPriority.Normal)
                    .Subscribe(n =>
                    {
                        ErrorMessage.Message = n.Message;
                    }));
            });
        }
    }
}