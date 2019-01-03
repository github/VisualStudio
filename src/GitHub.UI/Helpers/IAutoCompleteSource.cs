using System;
using GitHub.UI;

namespace GitHub.Helpers
{
    public interface IAutoCompleteSource
    {
        IObservable<AutoCompleteSuggestion> GetSuggestions();

        // The prefix used to trigger auto completion.
        string Prefix { get; }
    }
}
