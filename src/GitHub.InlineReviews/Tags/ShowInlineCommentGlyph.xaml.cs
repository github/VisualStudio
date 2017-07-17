using System;
using System.Windows.Controls;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;
using System.Linq;
using GitHub.Models;

namespace GitHub.InlineReviews.Tags
{
    public partial class ShowInlineCommentGlyph : UserControl
    {
        public ShowInlineCommentGlyph()
        {
            InitializeComponent();
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            var tag = Tag as ShowInlineCommentTag;
            var comments = tag.Thread.Comments.Select(comment => new PullRequestReviewCommentModel
            {
                User = comment.User,
                Body = comment.Body,
                CreatedAt = comment.CreatedAt
            });

            var viewModel = new TooltipCommentThreadViewModel(comments);
            var view = new TooltipCommentThreadView();
            view.DataContext = viewModel;

            CommentToolTip.Content = view;
        }

        protected override void OnToolTipClosing(ToolTipEventArgs e)
        {
            CommentToolTip.Content = null;
        }
    }
}
