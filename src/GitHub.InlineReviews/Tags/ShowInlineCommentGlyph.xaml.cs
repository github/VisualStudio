using System;
using System.Windows.Controls;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Tags
{
    public partial class ShowInlineCommentGlyph : UserControl
    {
        readonly CommentTooltipView commentTooltipView;

        public ShowInlineCommentGlyph()
        {
            InitializeComponent();

            commentTooltipView = new CommentTooltipView();
            ToolTip = commentTooltipView;
            ToolTipOpening += ShowInlineCommentGlyph_ToolTipOpening;
        }

        private void ShowInlineCommentGlyph_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var tag = Tag as ShowInlineCommentTag;

            var viewModel = new CommentTooltipViewModel();
            foreach (var comment in tag.Thread.Comments)
            {
                var commentViewModel = new TooltipCommentViewModel(comment.User, comment.Body, comment.CreatedAt);
                viewModel.Comments.Add(commentViewModel);
            }

            commentTooltipView.DataContext = viewModel;
        }
    }
}
