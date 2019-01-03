using System.Diagnostics.CodeAnalysis;
using GitHub.Cache;

namespace GitHub.Helpers
{
    /// <summary>
    /// Used to cache and supply @mentions in the autocomplete control.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces",
        Justification = "Yeah, it's empty, but it makes it easy to import the correct one.")]
    public interface IMentionsCache : IAutoCompleteSourceCache
    {
    }
}