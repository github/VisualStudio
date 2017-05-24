using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface IGlyphFactories<TGlyphTag> where TGlyphTag : ITag
    {
        IEnumerable<Type> GetTagTypes();
        IGlyphFactory<TGlyphTag> InstantiateGlyphFactory(Type glyphType, IWpfTextView textView, IWpfTextViewMargin margin);
    }
}
