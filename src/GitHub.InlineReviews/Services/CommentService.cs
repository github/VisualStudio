using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(ICommentService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CommentService:ICommentService
    {
        public bool ConfirmCommentDelete()
        {
            return MessageBox.Show(
                VisualStudio.UI.Resources.DeleteCommentConfirmation,
                VisualStudio.UI.Resources.DeleteCommentConfirmationCaption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}