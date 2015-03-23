using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using System.Windows.Media;
using GitHub.UI;

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    [TeamExplorerServiceInvitation(GitHubInvitationSectionId, GitHubInvitationSectionPriority)]
    public class GitHubInvitationSection : TeamExplorerInvitationBase
    {
        public const string GitHubInvitationSectionId = "C2443FCC-6D62-4D31-B08A-C4DE70109C7F";
        public const int GitHubInvitationSectionPriority = 100;

        public GitHubInvitationSection()
        {
            Name = "GitHub";
            Provider = "GitHub, Inc.";
            Icon = GetDrawingForIcon(GetBrushForIcon());

            IsVisible = true;

            VSColorTheme.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(ThemeChangedEventArgs e)
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
