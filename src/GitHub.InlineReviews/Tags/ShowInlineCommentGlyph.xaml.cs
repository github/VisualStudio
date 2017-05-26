using System;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.InlineReviews.Tags
{
    public partial class ShowInlineCommentGlyph : UserControl
    {
        public ShowInlineCommentGlyph()
        {
            InitializeComponent();

            Loaded += ShowInlineCommentGlyph_Loaded;
        }

        void ShowInlineCommentGlyph_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var backgroundColor = ImageThemingUtilities.GetImageBackgroundColor(this);
            Background = new SolidColorBrush(backgroundColor);
        }
    }
}
