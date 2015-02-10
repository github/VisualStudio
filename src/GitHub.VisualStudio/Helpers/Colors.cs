using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GitHub.VisualStudio.Helpers
{
    public static class Colors
    {
        public static Color RedNavigationItem = Color.FromRgb(240, 80, 51);
        public static Color BlueNavigationItem = Color.FromRgb(0, 121, 206);
        public static Color LightBlueNavigationItem = Color.FromRgb(25, 140, 205);

        public static int ToInt32(this Color color)
        {
            return BitConverter.ToInt32(new byte[]{ color.B, color.G, color.R, color.A }, 0);
        }
    }
}
