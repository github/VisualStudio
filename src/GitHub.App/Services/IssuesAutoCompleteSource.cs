using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;

namespace GitHub.Services
{
    [Export(typeof(IAutoCompleteSource))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IssuesAutoCompleteSource : IAutoCompleteSource
    {
        readonly ITeamExplorerContext teamExplorerContext;
        readonly IGraphQLClientFactory graphqlFactory;
        ICompiledQuery<Page<SuggestionItem>> query;

        [ImportingConstructor]
        public IssuesAutoCompleteSource(ITeamExplorerContext teamExplorerContext, IGraphQLClientFactory graphqlFactory)
        {
            Guard.ArgumentNotNull(teamExplorerContext, nameof(teamExplorerContext));
            Guard.ArgumentNotNull(graphqlFactory, nameof(graphqlFactory));

            this.teamExplorerContext = teamExplorerContext;
            this.graphqlFactory = graphqlFactory;
        }

        public IObservable<AutoCompleteSuggestion> GetSuggestions()
        {
            var localRepositoryModel = teamExplorerContext.ActiveRepository;

            var hostAddress = HostAddress.Create(localRepositoryModel.CloneUrl.Host);
            var owner = localRepositoryModel.Owner;
            var name = localRepositoryModel.Name;

            string filter;
            string after;

            if (query == null)
            {
               query = new Query().Search(query: Var(nameof(filter)), SearchType.Issue, 100, after: Var(nameof(after)))
                       .Select(item => new Page<SuggestionItem>
                       {
                           Items = item.Nodes.Select(searchResultItem => 
                               searchResultItem.Switch<SuggestionItem>(selector => selector
                                       .Issue(i => new SuggestionItem("#" + i.Number, i.Title) { LastModifiedDate = i.LastEditedAt })
                                       .PullRequest(p => new SuggestionItem("#" + p.Number, p.Title) { LastModifiedDate = p.LastEditedAt }))
                               ).ToList(),
                           EndCursor = item.PageInfo.EndCursor,
                           HasNextPage = item.PageInfo.HasNextPage,
                           TotalCount = item.IssueCount
                       })
                       .Compile();
            }

            filter = $"repo:{owner}/{name}";

            return Observable.FromAsync(async () =>
            {
                var results = new List<SuggestionItem>();

                var variables = new Dictionary<string, object>
                {
                    {nameof(filter), filter },
                };

                var connection = await graphqlFactory.CreateConnection(hostAddress);
                var searchResults = await connection.Run(query, variables);

                results.AddRange(searchResults.Items);

                while (searchResults.HasNextPage)
                {
                    variables[nameof(after)] = searchResults.EndCursor;
                    searchResults = await connection.Run(query, variables);

                    results.AddRange(searchResults.Items);
                }

                return results.Select(item => new IssueAutoCompleteSuggestion(item, Prefix));

            }).SelectMany(observable => observable);
        }

        class SearchResult
        {
            public SuggestionItem SuggestionItem { get; set; }
        }

        public string Prefix
        {
            get { return "#"; }
        }

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
