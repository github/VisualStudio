using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.ViewModels;
using Octokit;

namespace GitHub.Cache
{
    [Export(typeof(IIssuesCache))]
    [Export(typeof(IAutoCompleteSourceCache))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IssuesCache : AutoCompleteSourceCache, IIssuesCache
    {
        // Just needs to be some value before GitHub stored its first issue.
        static readonly DateTimeOffset lowerBound = new DateTimeOffset(2000, 1, 1, 12, 0, 0, TimeSpan.FromSeconds(0));

        [ImportingConstructor]
        public IssuesCache() : base(TimeSpan.FromSeconds(10), TimeSpan.FromDays(7))
        {
        }

        protected override string CacheSuffix
        {
            get { return "issues"; }
        }

        protected override IObservable<SuggestionItem> FetchSuggestionsSourceItems(
            CachedData<List<SuggestionItem>> existingCachedItems,
            IRepositoryModel repository,
            IApiClient apiClient)
        {
            var data = (existingCachedItems.Data ?? new List<SuggestionItem>())
                .Where(item => !String.IsNullOrEmpty(item.Name)) // Helps handle cache corruption
                .ToList();

            if (data.IsEmpty())
            {
                return apiClient.GetIssuesForRepository(repository.Owner, repository.Name)
                    .Select(ConvertToSuggestionItem);
            }

            // Update cache with changes
            var since = data.Max(issue => issue.LastModifiedDate ?? lowerBound).ToUniversalTime();
            var existingIssues = data.ToDictionary(i => i.Name, i => i);
            return apiClient.GetIssuesChangedSince(repository.Owner, repository.Name, since)
                .WhereNotNull()
                .Do(issue =>
                {
                    var suggestionItem = ConvertToSuggestionItem(issue);    
                    // Remove closed ones.
                    if (issue.State == ItemState.Closed)
                    {
                        existingIssues.Remove(suggestionItem.Name);
                    }
                    else
                    {
                        // Adds new ones (this is basically a noop for existing ones)
                        existingIssues[suggestionItem.Name] = suggestionItem;
                    }
                })
                .ToList() // We always want to return existing issues.
                .SelectMany(_ => existingIssues.Values.ToObservable());
        }

        static SuggestionItem ConvertToSuggestionItem(Issue issue)
        {
            return new SuggestionItem("#" + issue.Number, issue.Title)
            {
                // Just in case CreatedAt isn't set, we'll use UTCNow.
                LastModifiedDate = issue.UpdatedAt
                    ?? (issue.CreatedAt == DateTimeOffset.MinValue ? DateTimeOffset.UtcNow : issue.CreatedAt)
            };
        }
    }
}
