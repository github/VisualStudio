using System;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IAutoCompleteSource
    {
        IObservable<AutoCompleteSuggestion> GetSuggestions();

        // The prefix used to trigger auto completion.
        string Prefix { get; }
    }
}
