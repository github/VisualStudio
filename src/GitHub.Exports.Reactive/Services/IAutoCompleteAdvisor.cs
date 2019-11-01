using System;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IAutoCompleteAdvisor
    {
        IObservable<AutoCompleteResult> GetAutoCompletionSuggestions(string text, int caretPosition);
    }
}
