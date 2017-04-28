using System.Collections.Generic;
using System.Windows.Media;
using GitHub.UI;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio
{
    public static class SharedResources
    {
        static readonly Dictionary<string, DrawingBrush> drawingBrushes = new Dictionary<string, DrawingBrush>();

        public static DrawingBrush GetDrawingForIcon(Octicon icon, Color color, string theme = null)
        {
            return GetDrawingForIcon(icon, new SolidColorBrush(color).FreezeThis(), theme);
        }

        public static DrawingBrush GetDrawingForIcon(Octicon icon, Brush colorBrush, string theme = null)
        {
            string name = icon.ToString();
            if (theme != null)
                name += "_" + theme;
            if (drawingBrushes.ContainsKey(name))
                return drawingBrushes[name];

            var brush = new DrawingBrush()
            {
                Drawing = new GeometryDrawing()
                {
                    Brush = colorBrush,
                    Geometry = OcticonPath.GetGeometryForIcon(icon).FreezeThis()
                }
                .FreezeThis(),
                Stretch = Stretch.Uniform
            }
            .FreezeThis();
            drawingBrushes.Add(name, brush);
            return brush;
        }
    }
}
