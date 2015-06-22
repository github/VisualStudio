using System;
using System.Windows.Media;

namespace GitHub.VisualStudio.Helpers
{
    public static class Colors
    {
        public static Color RedNavigationItem        = Color.FromRgb(0xF0, 0x50, 0x33);
        public static Color BlueNavigationItem       = Color.FromRgb(0x00, 0x79, 0xCE);
        public static Color LightBlueNavigationItem  = Color.FromRgb(0x00, 0x9E, 0xCE);
        public static Color DarkPurpleNavigationItem = Color.FromRgb(0x68, 0x21, 0x7A);
        public static Color GrayNavigationItem       = Color.FromRgb(0x73, 0x82, 0x8C);
        public static Color YellowNavigationItem     = Color.FromRgb(0xF9, 0xC9, 0x00);
        public static Color PurpleNavigationItem     = Color.FromRgb(0xAE, 0x3C, 0xBA);

        public static Color LightThemeNavigationItem = Color.FromRgb(66, 66, 66);
        public static Color DarkThemeNavigationItem = Color.FromRgb(200, 200, 200);

        public static int ToInt32(this Color color)
        {
            return BitConverter.ToInt32(new byte[]{ color.B, color.G, color.R, color.A }, 0);
        }

        public static Color ToColor(this System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
