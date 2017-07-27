using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using GitHub.Settings;
using GitHub.Services;
using GitHub.InlineReviews.Tags;

namespace GitHub.InlineReviews.ViewModels
{
    public class GlyphMarginViewModel : IGlyphMarginViewModel
    {
        readonly IPackageSettings packageSettings;
        readonly IPullRequestSessionManager sessionManager;
        readonly IWpfTextViewHost wpfTextViewHost;
        readonly bool isDiffView;

        public GlyphMarginViewModel(IPackageSettings packageSettings, IPullRequestSessionManager sessionManager,
            IWpfTextViewHost wpfTextViewHost, bool isDiffView)
        {
            this.packageSettings = packageSettings;
            this.sessionManager = sessionManager;
            this.wpfTextViewHost = wpfTextViewHost;
            this.isDiffView = isDiffView;
        }

        public bool Visible
        {
            get
            {
                if (!isDiffView && !packageSettings.EditorComments)
                {
                    return false;
                }

                return IsMarginVisible(wpfTextViewHost.TextView.TextBuffer);
            }
        }

        bool IsMarginVisible(ITextBuffer buffer)
        {
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
