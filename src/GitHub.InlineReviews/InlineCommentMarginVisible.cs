using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews
{
    [Export(typeof(EditorOptionDefinition))]
    [Name(OptionName)]
    public class InlineCommentMarginVisible : ViewOptionDefinition<bool>
    {
        public const string OptionName = "TextViewHost/InlineCommentMarginVisible";

        public override bool Default => false;

        public override EditorOptionKey<bool> Key => new EditorOptionKey<bool>(OptionName);
    }
}
