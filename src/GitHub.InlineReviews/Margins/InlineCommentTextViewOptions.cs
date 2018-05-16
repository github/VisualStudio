using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Margins
{
    public static class InlineCommentTextViewOptions
    {
        public static EditorOptionKey<bool> MarginVisibleId = new EditorOptionKey<bool>("TextViewHost/InlineCommentMarginVisible");

        public static EditorOptionKey<bool> MarginEnabledId = new EditorOptionKey<bool>("TextViewHost/InlineCommentMarginEnabled");
    }
}
