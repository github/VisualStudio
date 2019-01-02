using System;

namespace GitHub.UI
{
    public interface IAutoCompleteAdvisor
    {
        IObservable<AutoCompleteResult> GetAutoCompletionSuggestions(string text, int caretPosition);
    }
}
