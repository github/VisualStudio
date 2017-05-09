using System;
using System.Windows.Controls;

namespace GitHub.InlineReviews.Tags
{
    public partial class AddInlineCommentGlyph : UserControl
    {
        public AddInlineCommentGlyph()
        {
            InitializeComponent();

            Visibility = System.Windows.Visibility.Hidden;
            MouseEnter += (s, e) => Visibility = System.Windows.Visibility.Visible;
            MouseLeave += (s, e) => Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
