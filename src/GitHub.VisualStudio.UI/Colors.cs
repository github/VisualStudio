using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Windows;
using System.Windows.Media;

namespace GitHub.VisualStudio.Helpers
{
    public static class Colors
    {
        public static readonly Color RedNavigationItem        = Color.FromRgb(0xF0, 0x50, 0x33);
        public static readonly Color BlueNavigationItem       = Color.FromRgb(0x00, 0x79, 0xCE);
        public static readonly Color LightBlueNavigationItem  = Color.FromRgb(0x00, 0x9E, 0xCE);
        public static readonly Color DarkPurpleNavigationItem = Color.FromRgb(0x68, 0x21, 0x7A);
        public static readonly Color GrayNavigationItem       = Color.FromRgb(0x73, 0x82, 0x8C);
        public static readonly Color YellowNavigationItem     = Color.FromRgb(0xF9, 0xC9, 0x00);
        public static readonly Color PurpleNavigationItem     = Color.FromRgb(0xAE, 0x3C, 0xBA);

        public static readonly Color LightThemeNavigationItem = Color.FromRgb(66, 66, 66);
        public static readonly Color DarkThemeNavigationItem = Color.FromRgb(200, 200, 200);

        public static int ToInt32(this Color color)
        {
            return BitConverter.ToInt32(new byte[]{ color.B, color.G, color.R, color.A }, 0);
        }

        public static Color ToColor(this System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }


        static readonly Color AccentMediumDarkTheme = Color.FromRgb(45, 45, 48);
        static readonly Color AccentMediumLightTheme = Color.FromRgb(238, 238, 242);
        static readonly Color AccentMediumBlueTheme = Color.FromRgb(255, 236, 181);

        public static string DetectTheme()
        {
            if (Application.Current?.TryFindResource(EnvironmentColors.AccentMediumColorKey) is Color cc)
            {
                if (cc == AccentMediumBlueTheme)
                    return "Blue";
                if (cc == AccentMediumLightTheme)
                    return "Light";
                if (cc == AccentMediumDarkTheme)
                    return "Dark";
                var color = System.Drawing.Color.FromArgb(cc.A, cc.R, cc.R, cc.B);
                var brightness = color.GetBrightness();
                var dark = brightness < 0.5f;
                return dark ? "Dark" : "Light";
            }

            // When Visual Studio resources aren't active
            return "Dark";
        }
    }
}
