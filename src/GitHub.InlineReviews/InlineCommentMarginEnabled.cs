using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews
{
    [Export(typeof(EditorOptionDefinition))]
    [Name(OptionName)]
    public class InlineCommentMarginEnabled : ViewOptionDefinition<bool>
    {
        public const string OptionName = "TextViewHost/InlineCommentMargin";

        public override bool Default => true;

        public override EditorOptionKey<bool> Key => new EditorOptionKey<bool>(OptionName);
    }
}
