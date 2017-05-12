using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using GitHub.Extensions;
using GitHub.VisualStudio.TeamExplorer.Sync;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.Services
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [Export(typeof(ITeamExplorerServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServices : ITeamExplorerServices
    {
        readonly IGitHubServiceProvider serviceProvider;

        /// <summary>
        /// This MEF export requires specific versions of TeamFoundation. ITeamExplorerNotificationManager is declared here so
        /// that instances of this type cannot be created if the TeamFoundation dlls are not available
        /// (otherwise we'll have multiple instances of ITeamExplorerServices exports, and that would be Bad(tm))
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        ITeamExplorerNotificationManager manager;

        [ImportingConstructor]
        public TeamExplorerServices(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void ShowPublishSection()
        {
            var te = serviceProvider.TryGetService<ITeamExplorer>();
            var foo = te.NavigateToPage(new Guid(TeamExplorerPageIds.GitCommits), null);
            var publish = foo?.GetSection(new Guid(GitHubPublishSection.GitHubPublishSectionId)) as GitHubPublishSection;
            publish?.Connect();
        }

        public void ShowMessage(string message)
        {
            manager = serviceProvider.GetService<ITeamExplorer, ITeamExplorerNotificationManager>();
            manager?.ShowNotification(message, NotificationType.Information, NotificationFlags.None, null, default(Guid));
        }

        public void ShowMessage(string message, ICommand command, bool showToolTips = true, Guid guid = default(Guid))
        {
            manager = serviceProvider.GetService<ITeamExplorer, ITeamExplorerNotificationManager>();
            manager?.ShowNotification(
                message,
                NotificationType.Information,
                showToolTips ? NotificationFlags.None : NotificationFlags.NoTooltips,
                command,
                guid);
        }

        public void ShowWarning(string message)
        {
            manager = serviceProvider.GetService<ITeamExplorer, ITeamExplorerNotificationManager>();
            manager?.ShowNotification(message, NotificationType.Warning, NotificationFlags.None, null, default(Guid));
        }

        public void ShowError(string message)
        {
            manager = serviceProvider.GetService<ITeamExplorer, ITeamExplorerNotificationManager>();
            manager?.ShowNotification(message, NotificationType.Error, NotificationFlags.None, null, default(Guid));
        }

        public void HideNotification(Guid guid)
        {
            manager = serviceProvider.GetService<ITeamExplorer, ITeamExplorerNotificationManager>();
            manager?.HideNotification(guid);
        }

        public void ClearNotifications()
        {
            manager = serviceProvider.GetService<ITeamExplorer, ITeamExplorerNotificationManager>();
            manager?.ClearNotifications();
        }

        public bool IsNotificationVisible(Guid guid)
        {
            manager = serviceProvider.GetService<ITeamExplorer, ITeamExplorerNotificationManager>();
            return manager?.IsNotificationVisible(guid) ?? false;
        }
    }
}