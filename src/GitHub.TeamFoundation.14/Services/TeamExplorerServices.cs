using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using GitHub.VisualStudio.TeamExplorer.Sync;
using Microsoft.TeamFoundation.Controls;
using ReactiveUI;

namespace GitHub.Services
{
    [Export(typeof(ITeamExplorerServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServices : ITeamExplorerServices
    {
        readonly IGitHubServiceProvider serviceProvider;

        readonly static Guid repositorySettingsPageId = new Guid("96903923-97e0-474a-9346-31a3ba28e6ff");
        readonly static Guid remotesSectionId = new Guid("2e31f317-7144-4316-8aae-a796e4be1fd4");

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
            var vsServices = serviceProvider.GetService<IVSServices>();
            vsServices.TryOpenRepository(repositoryPath);
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

        public async Task ShowRepositorySettingsRemotesAsync()
        {
            var te = serviceProvider.TryGetService<ITeamExplorer>();
            var page = await NavigateToPageAsync(te, repositorySettingsPageId);
            var remotes = page?.GetSection(remotesSectionId);
            await BringIntoViewAsync(remotes);
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

        void OpenFolder(string repositoryPath)
        {
            var dte = serviceProvider.TryGetService<DTE>();
            dte?.ExecuteCommand("File.OpenFolder", repositoryPath);
        }

        static async Task BringIntoViewAsync(ITeamExplorerSection section)
        {
            var content = section?.SectionContent as UserControl;
            if (section == null)
            {
                return;
            }

            // Wait for section content to load
            await Observable.FromEventPattern(content, nameof(content.Loaded))
                .Select(e => content.IsLoaded)
                .StartWith(content.IsLoaded)
                .Where(l => l == true)
                .Take(1);

            // Specify a tall rectangle to bring section to the top
            var targetRectangle = new Rect(0, 0, 0, 1000);
            content.BringIntoView(targetRectangle);
        }

        static async Task<ITeamExplorerPage> NavigateToPageAsync(ITeamExplorer teamExplorer, Guid pageId)
        {
            teamExplorer.NavigateToPage(pageId, null);
            var page = await teamExplorer
                .WhenAnyValue(x => x.CurrentPage)
                .Where(x => x?.GetId() == pageId)
                .Take(1);
            return page;
        }
    }
}