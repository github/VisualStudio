using GitHub.Info;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.Extensions;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using GitHub.VisualStudio.UI;
using System.Linq;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    [TeamExplorerServiceInvitation(GitHubInvitationSectionId, GitHubInvitationSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubInvitationSection : TeamExplorerInvitationBase
    {
        public const string GitHubInvitationSectionId = "C2443FCC-6D62-4D31-B08A-C4DE70109C7F";
        public const int GitHubInvitationSectionPriority = 100;
        readonly IDialogService dialogService;
        readonly Lazy<IVisualStudioBrowser> lazyBrowser;

        [ImportingConstructor]
        public GitHubInvitationSection(
            IGitHubServiceProvider serviceProvider,
            IDialogService dialogService,
            IConnectionManager cm,
            Lazy<IVisualStudioBrowser> browser)
            : base(serviceProvider)
        {
            this.dialogService = dialogService;
            lazyBrowser = browser;
            CanConnect = true;
            CanSignUp = true;
            ConnectLabel = Resources.GitHubInvitationSectionConnectLabel;
            SignUpLabel = Resources.SignUpLink;
            Name = "GitHub";
            Provider = "GitHub, Inc.";
            Description = Resources.BlurbText;
            OnThemeChanged();
            VSColorTheme.ThemeChanged += _ =>
            {
                OnThemeChanged();
            };

            IsVisible = !cm.Connections.Where(x => x.IsLoggedIn).Any();

            cm.Connections.CollectionChanged += (s, e) => IsVisible = !cm.Connections.Where(x => x.IsLoggedIn).Any();
        }

        public override void Connect()
        {
            dialogService.ShowLoginDialog();
        }

        public override void SignUp()
        {
            OpenInBrowser(lazyBrowser, GitHubUrls.Plans);
        }

        void OnThemeChanged()
        {
            var theme = Helpers.Colors.DetectTheme();
            var dark = theme == "Dark";
            Icon = SharedResources.GetDrawingForIcon(Octicon.mark_github, dark ? Colors.White : Helpers.Colors.LightThemeNavigationItem, theme);
        }
    }
}
