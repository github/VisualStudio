using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Margins
{
    [Export(typeof(EditorOptionDefinition))]
    public class InlineCommentMarginVisible : ViewOptionDefinition<bool>
    {
        public override bool Default => false;

        public override EditorOptionKey<bool> Key => InlineCommentTextViewOptions.MarginVisibleId;
    }
}
