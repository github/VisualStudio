using System;
using System.Linq;
using System.Windows.Controls;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Tags
{
    public partial class ShowInlineCommentGlyph : UserControl
    {
        readonly CommentView tooltipCommentView;

        public ShowInlineCommentGlyph()
        {
            InitializeComponent();

            tooltipCommentView = new CommentView();
            ToolTip = tooltipCommentView;
            ToolTipOpening += ShowInlineCommentGlyph_ToolTipOpening;
        }

        private void ShowInlineCommentGlyph_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var tag = Tag as ShowInlineCommentTag;
            var comment = tag?.Thread.Comments.FirstOrDefault();
            if (comment == null)
            {
                return;
            }

            var viewModel = new SampleData.CommentViewModelDesigner();
            viewModel.EditState = CommentEditState.None;
            viewModel.Body = comment.Body;
            viewModel.User = comment.User;
            tooltipCommentView.DataContext = viewModel;
        }
    }
}
