using System.Collections.Generic;

namespace GitHub.Models
{
    public class AutoCompleteResult
    {
        public static AutoCompleteResult Empty = new AutoCompleteResult(0, new AutoCompleteSuggestion[] {});

        public AutoCompleteResult(int offset, IReadOnlyList<AutoCompleteSuggestion> suggestions)
        {
            Offset = offset;
            Suggestions = suggestions;
        }

        public int Offset { get; private set; }
        public IReadOnlyList<AutoCompleteSuggestion> Suggestions { get; private set; }
    }
}
