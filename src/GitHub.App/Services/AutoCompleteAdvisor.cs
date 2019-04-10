using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using Serilog;

namespace GitHub.Services
{
    [Export(typeof(IAutoCompleteAdvisor))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AutoCompleteAdvisor : IAutoCompleteAdvisor
    {
        const int SuggestionCount = 5; // The number of suggestions we'll provide. github.com does 5.

        static readonly ILogger log = LogManager.ForContext<AutoCompleteAdvisor>();
        readonly Lazy<Dictionary<string, IAutoCompleteSource>> prefixSourceMap;

        [ImportingConstructor]
        public AutoCompleteAdvisor([ImportMany(typeof(IAutoCompleteSource))]IEnumerable<IAutoCompleteSource> autocompleteSources)
        {
            prefixSourceMap = new Lazy<Dictionary<string, IAutoCompleteSource>>(
                () => autocompleteSources.ToDictionary(s => s.Prefix, s => s));
        }

        public IObservable<AutoCompleteResult> GetAutoCompletionSuggestions(string text, int caretPosition)
        {
            Guard.ArgumentNotNull("text", text);

            if (caretPosition < 0 || caretPosition > text.Length)
            {
                string error = String.Format(CultureInfo.InvariantCulture,
                    "The CaretPosition '{0}', is not in the range of '0' and the text length '{1}' for the text '{2}'",
                    caretPosition,
                    text.Length,
                    text);

                // We need to be alerted when this happens because it should never happen.
                // But it apparently did happen in production.
                Debug.Fail(error);
                log.Error(error);
                return Observable.Empty<AutoCompleteResult>();
            }
            var tokenAndSource = PrefixSourceMap
                .Select(kvp => new {Source = kvp.Value, Token = ParseAutoCompletionToken(text, caretPosition, kvp.Key)})
                .FirstOrDefault(s => s.Token != null);

            if (tokenAndSource == null)
            {
                return Observable.Return(AutoCompleteResult.Empty);
            }

            return tokenAndSource.Source.GetSuggestions()
                .Select(suggestion => new
                {
                    suggestion,
                    rank = suggestion.GetSortRank(tokenAndSource.Token.SearchSearchPrefix)
                })
                .Where(suggestion => suggestion.rank > -1)
                .ToList()
                .Select(suggestions => suggestions
                    .OrderByDescending(s => s.rank)
                    .ThenBy(s => s.suggestion.Name)
                    .Take(SuggestionCount)
                    .Select(s => s.suggestion)
                    .ToList())
                .Select(suggestions => new AutoCompleteResult(tokenAndSource.Token.Offset,
                    new ReadOnlyCollection<AutoCompleteSuggestion>(suggestions)))
                .Catch<AutoCompleteResult, Exception>(e =>
                {
                    log.Error(e, "Error Getting AutoCompleteResult");
                    return Observable.Return(AutoCompleteResult.Empty);
                });
        }

        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "caretPosition-1"
            , Justification = "We ensure the argument is greater than -1 so it can't overflow")]
        public static AutoCompletionToken ParseAutoCompletionToken(string text, int caretPosition, string triggerPrefix)
        {
            Guard.ArgumentNotNull("text", text);
            Guard.ArgumentInRange(caretPosition, 0, text.Length, "caretPosition");
            if (caretPosition == 0 || text.Length == 0) return null;

            // :th     : 1
            //:th      : 0
            //Hi :th   : 3
            int beginningOfWord = text.LastIndexOfAny(new[] { ' ', '\n' }, caretPosition - 1) + 1;
            string word = text.Substring(beginningOfWord, caretPosition - beginningOfWord);
            if (!word.StartsWith(triggerPrefix, StringComparison.Ordinal)) return null;

            return new AutoCompletionToken(word.Substring(1), beginningOfWord);
        }

        Dictionary<string, IAutoCompleteSource> PrefixSourceMap { get { return prefixSourceMap.Value; } }
    }

    public class AutoCompletionToken
    {
        public AutoCompletionToken(string searchPrefix, int offset)
        {
            Guard.ArgumentNotNull(searchPrefix, "searchPrefix");
            Guard.ArgumentNonNegative(offset, "offset");

            SearchSearchPrefix = searchPrefix;
            Offset = offset;
        }

        /// <summary>
        /// Used to filter the list of auto complete suggestions to what the user has typed in.
        /// </summary>
        public string SearchSearchPrefix { get; private set; }
        public int Offset { get; private set; }
    }
}
