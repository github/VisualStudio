using GitHub.Info;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    [TeamExplorerServiceInvitation(GitHubInvitationSectionId, GitHubInvitationSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubInvitationSection : TeamExplorerInvitationBase
    {
        public const string GitHubInvitationSectionId = "C2443FCC-6D62-4D31-B08A-C4DE70109C7F";
        public const int GitHubInvitationSectionPriority = 100;
        readonly Lazy<IVisualStudioBrowser> lazyBrowser;

        [ImportingConstructor]
        public GitHubInvitationSection(IConnectionManager cm, Lazy<IVisualStudioBrowser> browser)
        {
            lazyBrowser = browser;
            CanConnect = true;
            CanSignUp = true;
            ConnectLabel = Resources.GitHubInvitationSectionConnectLabel;
            SignUpLabel = Resources.GitHubInvitationSectionSignUpLabel;
            Name = "GitHub";
            Provider = "GitHub, Inc.";
            Description = Resources.GitHubInvitationSectionDescription;
            Icon = GetDrawingForIcon(GetBrushForIcon());

            IsVisible = cm.Connections.Count == 0;

            cm.Connections.CollectionChanged += (s, e) => IsVisible = cm.Connections.Count == 0;
            VSColorTheme.ThemeChanged += OnThemeChanged;
        }

        public override void Connect()
        {
            StartFlow(UIControllerFlow.Authentication);
            base.Connect();
        }

        public override void SignUp()
        {
            OpenInBrowser(lazyBrowser, GitHubUrls.Plans);
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.RunUI(controllerFlow, null);
        }

        void OnThemeChanged(ThemeChangedEventArgs e)
        {
            Icon = GetDrawingForIcon(GetBrushForIcon());
        }

        static Brush GetBrushForIcon()
        {
            var brush = Application.Current.TryFindResource(HeaderColors.DefaultTextBrushKey) as Brush;
            if (brush == null)
                brush = Brushes.Black;
            return brush;
        }

        static DrawingBrush GetDrawingForIcon(Brush color)
        {
            return SharedResources.GetDrawingForIcon(Octicon.mark_github, color);
        }
    }
}
