using System;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews
{
    static class VisualStudioExtensions
    {
        public static T GetOptionValue<T>(this IEditorOptions options, string optionId, T defaultValue)
        {
            return options.IsOptionDefined(optionId, false) ?
                options.GetOptionValue<T>(optionId) : defaultValue;
        }
    }
}
