using System;
using System.Windows.Controls;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Tags
{
    public partial class ShowInlineCommentGlyph : UserControl
    {
        readonly ToolTip toolTip;

        public ShowInlineCommentGlyph()
        {
            InitializeComponent();

            toolTip = new ToolTip();
            ToolTip = toolTip;
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            var tag = Tag as ShowInlineCommentTag;

            var viewModel = new CommentTooltipViewModel();
            foreach (var comment in tag.Thread.Comments)
            {
                var commentViewModel = new TooltipCommentViewModel(comment.User, comment.Body, comment.CreatedAt);
                viewModel.Comments.Add(commentViewModel);
            }

            var view = new CommentTooltipView();
            view.DataContext = viewModel;

            toolTip.Content = view;
        }

        protected override void OnToolTipClosing(ToolTipEventArgs e)
        {
            toolTip.Content = null;
        }
    }
}
