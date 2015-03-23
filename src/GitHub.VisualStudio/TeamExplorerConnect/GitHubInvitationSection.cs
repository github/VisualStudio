using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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

            IsVisible = true;

            var brush = Application.Current.TryFindResource(HeaderColors.DefaultTextBrushKey) as Brush;
            if (brush == null)
                brush = Brushes.Black;
            Icon = GetDrawingForIcon(brush);

            VSColorTheme.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(ThemeChangedEventArgs e)
        {
            var brush = Application.Current.TryFindResource(HeaderColors.DefaultTextBrushKey) as Brush;
            if (brush == null)
                brush = Brushes.Black;
            Icon = GetDrawingForIcon(brush);
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
