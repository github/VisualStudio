using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Cache;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels;

namespace GitHub.Helpers
{
    [Export(typeof(IAutoCompleteSource))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IssuesAutoCompleteSource : IAutoCompleteSource
    {
        readonly Lazy<IIssuesCache> issuesCache;
        readonly Lazy<ISourceListViewModel> currentRepositoryState;

        [ImportingConstructor]
        public IssuesAutoCompleteSource(
            Lazy<IIssuesCache> issuesCache,
            Lazy<ISourceListViewModel> currentRepositoryState)
        {
            Ensure.ArgumentNotNull(issuesCache, "issuesCache");
            Ensure.ArgumentNotNull(currentRepositoryState, "currentRepositoryState");

            this.issuesCache = issuesCache;
            this.currentRepositoryState = currentRepositoryState;
        }

        public IObservable<AutoCompleteSuggestion> GetSuggestions()
        {
            if (CurrentRepository.RepositoryHost == null)
            {
                return Observable.Empty<AutoCompleteSuggestion>();
            }

            return IssuesCache.RetrieveSuggestions(CurrentRepository)
                .Catch<IReadOnlyList<SuggestionItem>, Exception>(_ => Observable.Empty<IReadOnlyList<SuggestionItem>>())
                .SelectMany(x => x.ToObservable())
                .Where(suggestion => !String.IsNullOrEmpty(suggestion.Name)) // Just being extra cautious
                .Select(suggestion => new IssueAutoCompleteSuggestion(suggestion, Prefix));
        }

        public string Prefix
        {
            get { return "#"; }
        }

        IIssuesCache IssuesCache { get { return issuesCache.Value; } }

        IRepositoryModel CurrentRepository { get { return currentRepositoryState.Value.SelectedRepository; } }

        class IssueAutoCompleteSuggestion : AutoCompleteSuggestion
        {
            // Just needs to be some value before GitHub stored its first issue.
            static readonly DateTimeOffset lowerBound = new DateTimeOffset(2000, 1, 1, 12, 0, 0, TimeSpan.FromSeconds(0));

            readonly SuggestionItem suggestion;
            public IssueAutoCompleteSuggestion(SuggestionItem suggestion, string prefix)
                : base(suggestion.Name, suggestion.Description, prefix)
            {
                this.suggestion = suggestion;
            }

            public override int GetSortRank(string text)
            {
                // We need to override the sort rank behavior because when we display issues, we include the prefix
                // unlike mentions. So we need to account for that in how we do filtering.
                if (text.Length == 0)
                {
                    return (int) ((suggestion.LastModifiedDate ?? lowerBound) - lowerBound).TotalSeconds;
                }
                // Name is always "#" followed by issue number.
                return Name.StartsWith("#" + text, StringComparison.OrdinalIgnoreCase)
                        ? 1
                        : DescriptionWords.Any(word => word.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                            ? 0
                            : -1;
            }

            // This is what gets "completed" when you tab.
            public override string ToString()
            {
                return Name;
            }
        }
    }
}
