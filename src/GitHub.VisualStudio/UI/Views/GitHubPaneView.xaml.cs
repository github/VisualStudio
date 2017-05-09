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
    public class GenericGitHubPaneView : ViewBase<IGitHubPaneViewModel, GitHubPaneView>
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
                infoPanel.Visibility = Visibility.Collapsed;
                d(notifications.Listen()
                    .ObserveOnDispatcher(DispatcherPriority.Normal)
                    .Subscribe(n =>
                    {
                        if (n.Type == Notification.NotificationType.Error || n.Type == Notification.NotificationType.Warning)
                            infoPanel.MessageType = MessageType.Warning;
                        else
                            infoPanel.MessageType = MessageType.Information;
                        infoPanel.Message = n.Message;
                    }));
            });
        }
    }
}