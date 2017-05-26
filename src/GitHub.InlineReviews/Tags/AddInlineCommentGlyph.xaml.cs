using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.InlineReviews.Tags
{
    public partial class AddInlineCommentGlyph : UserControl
    {
        public AddInlineCommentGlyph()
        {
            InitializeComponent();

            Loaded += AddInlineCommentGlyph_Loaded;

            AddViewbox.Visibility = Visibility.Hidden;
            MouseEnter += (s, e) => AddViewbox.Visibility = Visibility.Visible;
            MouseLeave += (s, e) => AddViewbox.Visibility = Visibility.Hidden;
        }

        void AddInlineCommentGlyph_Loaded(object sender, RoutedEventArgs e)
        {
            var backgroundColor = ImageThemingUtilities.GetImageBackgroundColor(this);
            Background = new SolidColorBrush(backgroundColor);
        }
    }
}
