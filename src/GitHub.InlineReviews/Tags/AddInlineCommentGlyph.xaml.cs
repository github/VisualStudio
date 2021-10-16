using System;
using System.Windows;
using System.Windows.Controls;

namespace GitHub.InlineReviews.Tags
{
    public partial class AddInlineCommentGlyph : UserControl
    {
        public AddInlineCommentGlyph()
        {
            InitializeComponent();

            AddViewbox.Visibility = Visibility.Hidden;
            MouseEnter += (s, e) => AddViewbox.Visibility = Visibility.Visible;
            MouseLeave += (s, e) => AddViewbox.Visibility = Visibility.Hidden;
        }
    }
}
