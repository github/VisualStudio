using System;
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.VisualStudio.Text.Editor;
using GitHub.Settings;
using GitHub.Services;
using GitHub.InlineReviews.Tags;

namespace GitHub.InlineReviews.ViewModels
{
    public sealed class GlyphMarginViewModel : IGlyphMarginViewModel, IDisposable
    {
        readonly IPackageSettings packageSettings;
        readonly IPullRequestSessionManager sessionManager;
        readonly IWpfTextViewHost wpfTextViewHost;
        readonly bool isDiffView;
        readonly DispatcherTimer dispatcherTimer;

        bool visible;
        bool disposed;

        public GlyphMarginViewModel(IPackageSettings packageSettings, IPullRequestSessionManager sessionManager,
            IWpfTextViewHost wpfTextViewHost, bool isDiffView)
        {
            this.packageSettings = packageSettings;
            this.sessionManager = sessionManager;
            this.wpfTextViewHost = wpfTextViewHost;
            this.isDiffView = isDiffView;

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) => RefreshVisibility();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            packageSettings.PropertyChanged += PackageSettings_PropertyChanged;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                disposed = true;
                dispatcherTimer.Stop();
                packageSettings.PropertyChanged -= PackageSettings_PropertyChanged;
            }
        }

        void PackageSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(packageSettings.EditorComments))
            {
                RefreshVisibility();
            }
        }

        void RefreshVisibility()
        {
            Visible = IsVisible();
        }

        bool IsVisible()
        {
            if (!isDiffView && !packageSettings.EditorComments)
            {
                return false;
            }

            var buffer = wpfTextViewHost.TextView.TextBuffer;
            if (sessionManager.GetTextBufferInfo(buffer) != null)
            {
                return true;
            }

            InlineCommentTagger inlineCommentTagger;
            if (buffer.Properties.TryGetProperty(typeof(InlineCommentTagger), out inlineCommentTagger))
            {
                if (inlineCommentTagger.ShowMargin)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Visible
        {
            private set
            {
                if (value != visible)
                {
                    visible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visible)));
                }
            }
            get
            {
                return visible;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
