using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Margins
{
    [Export(typeof(EditorOptionDefinition))]
    [Name(OptionName)]
    public class InlineCommentMarginEnabled : ViewOptionDefinition<bool>
    {
        public const string OptionName = "TextViewHost/InlineCommentMarginEnabled";

        public override bool Default => false;

        public override EditorOptionKey<bool> Key => new EditorOptionKey<bool>(OptionName);
    }
}
