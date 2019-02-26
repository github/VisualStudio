using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;

namespace GitHub.Services
{
    /// <summary>
    /// Supplies @mentions auto complete suggestions.
    /// </summary>
    [Export(typeof(IAutoCompleteSource))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MentionsAutoCompleteSource : IAutoCompleteSource
    {
        readonly ITeamExplorerContext teamExplorerContext;
        readonly IGraphQLClientFactory graphqlFactory;
        readonly IAvatarProvider avatarProvider;

        [ImportingConstructor]
        public MentionsAutoCompleteSource(ITeamExplorerContext teamExplorerContext, 
            IGraphQLClientFactory graphqlFactory,
            IAvatarProvider avatarProvider)
        {
            Guard.ArgumentNotNull(teamExplorerContext, nameof(teamExplorerContext));
            Guard.ArgumentNotNull(graphqlFactory, nameof(graphqlFactory));
            Guard.ArgumentNotNull(avatarProvider, nameof(avatarProvider));

            this.teamExplorerContext = teamExplorerContext;
            this.graphqlFactory = graphqlFactory;
            this.avatarProvider = avatarProvider;
        }

        public IObservable<AutoCompleteSuggestion> GetSuggestions()
        {
            var localRepositoryModel = teamExplorerContext.ActiveRepository;
            var query = new Query().Repository(owner: localRepositoryModel.Owner, name: localRepositoryModel.Name)
                .Select(repository =>
                    repository.MentionableUsers(null, null, null, null)
                        .AllPages()
                        .Select(sourceItem => 
                            new SuggestionItem(sourceItem.Login, 
                                sourceItem.Name ?? "(unknown)", 
                                GetUrlSafe(sourceItem.AvatarUrl(null))))
                        .ToList());

            return Observable.FromAsync(async () =>
            {
                var connection = await graphqlFactory.CreateConnection(HostAddress.Create(localRepositoryModel.CloneUrl.Host));
                var suggestions = await connection.Run(query);
                return suggestions.Select(suggestion => new AutoCompleteSuggestion(suggestion.Name,
                    suggestion.Description,
                    ResolveImage(suggestion.IconKey.ToString()),
                    Prefix));
            }).SelectMany(enumerable => enumerable);
        }

        private IObservable<BitmapSource> ResolveImage(string uri) => avatarProvider.GetAvatar(uri);

        public string Prefix => "@";

        static Uri GetUrlSafe(string url)
        {
            Uri uri;
            Uri.TryCreate(url, UriKind.Absolute, out uri);
            return uri;
        }
    }
}
