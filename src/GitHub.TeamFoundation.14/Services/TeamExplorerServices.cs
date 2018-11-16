using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using EnvDTE;
using GitHub.VisualStudio;
using GitHub.VisualStudio.TeamExplorer.Sync;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.Services
{
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
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        ITeamExplorerNotificationManager manager;

        [ImportingConstructor]
        public TeamExplorerServices(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void OpenRepository(string repositoryPath)
        {
#if TEAMEXPLORER14
            SetActiveRepository(repositoryPath);
#else
            OpenFolder(repositoryPath);
#endif
        }

        public void ShowConnectPage()
        {
            var te = serviceProvider.TryGetService<ITeamExplorer>();
            te.NavigateToPage(new Guid(TeamExplorerPageIds.Connect), null);
        }

        public void ShowHomePage()
        {
            var te = serviceProvider.TryGetService<ITeamExplorer>();
            te.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
        }

        public void ShowPublishSection()
        {
            var te = serviceProvider.TryGetService<ITeamExplorer>();
            var page = te.NavigateToPage(new Guid(TeamExplorerPageIds.GitCommits), null);
            var publish = page?.GetSection(new Guid(GitHubPublishSection.GitHubPublishSectionId)) as GitHubPublishSection;
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

        public void SetActiveRepository(string repositoryPath, bool silent = false)
        {
            var sccUIService = FindSccUIService();
            var method = sccUIService.GetType().GetMethod("SetActiveRepository", new[] { typeof(string), typeof(bool) });
            method.Invoke(sccUIService, new object[] { repositoryPath, silent });
        }

        object FindSccUIService()
        {
            var sccServiceProvider = FindSccServiceProvider();
            var sccUIServiceType = typeof(IGitRepositoriesExt).Assembly.GetType("Microsoft.TeamFoundation.Git.Controls.ISccUIService", false);
            return sccServiceProvider.GetService(sccUIServiceType);
        }

        IServiceProvider FindSccServiceProvider()
        {
            var shell = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
            ErrorHandler.ThrowOnFailure(shell.LoadPackage(Guids.SccProviderPackageGuid, out var sccProviderPackage));
            return sccProviderPackage as IServiceProvider;
        }

        void OpenFolder(string repositoryPath)
        {
            var dte = serviceProvider.TryGetService<DTE>();
            dte?.ExecuteCommand("File.OpenFolder", repositoryPath);
        }
    }
}