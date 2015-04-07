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

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    [TeamExplorerServiceInvitation(GitHubInvitationSectionId, GitHubInvitationSectionPriority)]
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
            ConnectLabel = "Connect";
            SignUpLabel = "Sign up";
            Name = "GitHub";
            Provider = "GitHub, Inc.";
            Description = "Powerful collaboration, code review, and code management for open source and private projects.";
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

        Brush GetBrushForIcon()
        {
            var brush = Application.Current.TryFindResource(HeaderColors.DefaultTextBrushKey) as Brush;
            if (brush == null)
                brush = Brushes.Black;
            return brush;
        }

        DrawingBrush GetDrawingForIcon(Brush color)
        {
            Octicon icon = Octicon.mark_github;
            return new DrawingBrush() {
                Drawing = new GeometryDrawing()
                {
                    Brush = color,
                    Pen = new Pen(color, 1.0).FreezeThis(),
                    Geometry = OcticonPath.GetGeometryForIcon(icon).FreezeThis()
                }
                .FreezeThis(),
                Stretch = Stretch.Uniform
            }
            .FreezeThis();
        }
    }
}
