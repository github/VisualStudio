using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Formatting;

namespace GitHub.InlineReviews.Glyph
{
    /// <summary>
    /// A factory for a type of tag (or multiple subtypes).
    /// </summary>
    /// <typeparam name="TGlyphTag"></typeparam>
    public interface IGlyphFactory<TGlyphTag> where TGlyphTag : ITag
    {
        /// <summary>
        /// Create a glyph element for a particular line and tag.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        UIElement GenerateGlyph(IWpfTextViewLine line, TGlyphTag tag);

        /// <summary>
        /// A list of tag subtypes this is a factory for.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetTagTypes();
    }
}
