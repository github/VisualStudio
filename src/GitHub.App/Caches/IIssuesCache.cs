using System.Diagnostics.CodeAnalysis;
namespace GitHub.Cache
{
    /// <summary>
    /// Used to cache and supply #issues in the autocomplete control.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces",
        Justification = "Yeah, it's empty, but it makes it easy to import the correct one.")]
    public interface IIssuesCache : IAutoCompleteSourceCache
    {
    }
}