using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Forms;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(ICommentService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CommentService:ICommentService
    {
        public bool ConfirmCommentDelete()
        {
            var options = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ?
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign :
                0;

            return MessageBox.Show(
                Resources.DeleteCommentConfirmation,
                Resources.DeleteCommentConfirmationCaption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1,
                options) == DialogResult.Yes;
        }
    }
}