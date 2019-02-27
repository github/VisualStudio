using System;
using System.Windows;
using System.ComponentModel;
using Microsoft.VisualStudio.PlatformUI;
using GitHub.VisualStudio.Helpers;
using GitHub.UI.Helpers;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace GitHub.VisualStudio.UI.Helpers
{
    public class ThemeDictionaryManager : SharedDictionaryManager, IDisposable
    {
        static bool isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        Uri baseThemeUri;

        public override Uri Source
        {
            get { return base.Source; }
            set
            {
                InitTheme(value);
                base.Source = GetCurrentThemeUri();
            }
        }

        void InitTheme(Uri themeUri)
        {
            if (baseThemeUri == null)
            {
                baseThemeUri = themeUri;
                if (!isInDesignMode)
                {
                    VSColorTheme.ThemeChanged += OnThemeChange;
                }
            }
        }

        void OnThemeChange(ThemeChangedEventArgs e)
        {
            base.Source = GetCurrentThemeUri();
        }

        Uri GetCurrentThemeUri()
        {
            if (isInDesignMode)
            {
                return baseThemeUri;
            }

            var currentTheme = Colors.DetectTheme();
            return new Uri(baseThemeUri, "Theme" + currentTheme + ".xaml");
        }

        bool disposed;
        private void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                disposed = true;
                if (baseThemeUri != null)
                {
                    baseThemeUri = null;
                    if (!isInDesignMode)
                    {
                        VSColorTheme.ThemeChanged -= OnThemeChange;
                    }
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}