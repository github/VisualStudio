using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;

namespace GitHub.Services
{
    [Export(typeof(IAutoCompleteSource))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IssuesAutoCompleteSource : IAutoCompleteSource
    {
        readonly ITeamExplorerContext teamExplorerContext;
        readonly IGraphQLClientFactory graphqlFactory;

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
            var query = new Query().Repository(owner: localRepositoryModel.Owner, name: localRepositoryModel.Name)
                .Select(repository =>
                    repository.Issues(null, null, null, null, null, null, null)
                        .AllPages()
                        .Select(issue => new SuggestionItem("#" + issue.Number, issue.Title))
                        .ToList());

            return Observable.FromAsync(async () =>
                {
                    var connection = await graphqlFactory.CreateConnection(HostAddress.Create(localRepositoryModel.CloneUrl.Host));
                    var suggestions = await connection.Run(query);
                    return suggestions.Select(suggestion => new IssueAutoCompleteSuggestion(suggestion, Prefix));
                }).SelectMany(enumerable => enumerable);
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
