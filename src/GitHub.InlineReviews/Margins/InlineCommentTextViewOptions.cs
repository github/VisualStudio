using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Margins
{
    public static class InlineCommentTextViewOptions
    {
        public static readonly EditorOptionKey<bool> MarginVisibleId = new EditorOptionKey<bool>("TextViewHost/InlineCommentMarginVisible");

        public static readonly EditorOptionKey<bool> MarginEnabledId = new EditorOptionKey<bool>("TextViewHost/InlineCommentMarginEnabled");
    }
}
