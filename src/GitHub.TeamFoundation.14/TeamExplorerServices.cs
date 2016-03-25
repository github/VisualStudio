using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using GitHub.Extensions;

namespace GitHub.Services
{
    [Export(typeof(ITeamExplorerServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServices : ITeamExplorerServices
    {
        readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public TeamExplorerServices([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void ShowMessage(string message)
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ShowNotification(message, NotificationType.Information, NotificationFlags.None, null, default(Guid));
        }

        public void ShowMessage(string message, ICommand command)
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ShowNotification(message, NotificationType.Information, NotificationFlags.None, command, default(Guid));
        }

        public void ShowWarning(string message)
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ShowNotification(message, NotificationType.Warning, NotificationFlags.None, null, default(Guid));
        }

        public void ShowError(string message)
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ShowNotification(message, NotificationType.Error, NotificationFlags.None, null, default(Guid));
        }

        public void ClearNotifications()
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ClearNotifications();
        }
    }
}