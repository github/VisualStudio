using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Helpers;
using GitHub.Models;
using Octokit;

namespace GitHub.Cache
{
    /// <summary>
    /// Used to cache and supply @mentions in the autocomplete control.
    /// </summary>
    [Export(typeof(IMentionsCache))]
    [Export(typeof(IAutoCompleteSourceCache))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MentionsCache :  AutoCompleteSourceCache, IMentionsCache
    {
        public MentionsCache() : base(TimeSpan.FromHours(12), TimeSpan.FromDays(7))
        {
        }

        protected override string CacheSuffix
        {
            get { return "mentions"; }
        }

        protected override IObservable<SuggestionItem> FetchSuggestionsSourceItems(
            CachedData<List<SuggestionItem>> existingCachedItems,
            IRepositoryModel repository,
            IApiClient apiClient)
        {
            return apiClient.GetMentionables(repository.Owner, repository.Name)
                .Select(ConvertToSuggestionItem);
        }

        static SuggestionItem ConvertToSuggestionItem(AccountMention sourceItem)
        {
            return new SuggestionItem(sourceItem.Login, sourceItem.Name ?? "(unknown)", GetUrlSafe(sourceItem.AvatarUrl));
        }

        static Uri GetUrlSafe(string url)
        {
            Uri uri;
            Uri.TryCreate(url, UriKind.Absolute, out uri);
            return uri;
        }
    }
}
