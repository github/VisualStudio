using System;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews
{
    static class VisualStudioExtensions
    {
        public static T GetOptionValue<T>(this IEditorOptions options, string optionId, T defaultValue)
        {
            return options.IsOptionDefined(optionId, false) ?
                options.GetOptionValue<T>(optionId) : defaultValue;
        }

        public static T GetProperty<T>(this PropertyCollection properties, object key, T defaultValue)
        {
            T value;
            return properties.TryGetProperty(key, out value) ? value : defaultValue;
        }
    }
}
