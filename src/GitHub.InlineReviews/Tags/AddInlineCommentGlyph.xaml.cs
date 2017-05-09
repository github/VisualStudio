using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHub.InlineReviews.Tags
{
    public partial class AddInlineCommentGlyph : UserControl
    {
        public AddInlineCommentGlyph()
        {
            InitializeComponent();
            Visibility = System.Windows.Visibility.Hidden;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            Visibility = System.Windows.Visibility.Visible;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Visibility = System.Windows.Visibility.Hidden;
            base.OnMouseLeave(e);
        }
    }
}
