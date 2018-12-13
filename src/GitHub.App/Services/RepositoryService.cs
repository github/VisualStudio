using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Primitives;
using Octokit.GraphQL;
using static Octokit.GraphQL.Variable;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryService : IRepositoryService
    {
        static ICompiledQuery<Tuple<string, string>> readParentOwnerLogin;
        readonly IGraphQLClientFactory graphqlFactory;

        [ImportingConstructor]
        public RepositoryService(IGraphQLClientFactory graphqlFactory)
        {
            Guard.ArgumentNotNull(graphqlFactory, nameof(graphqlFactory));

            this.graphqlFactory = graphqlFactory;
        }

        public async Task<(string owner, string name)?> FindParent(HostAddress address, string owner, string name)
        {
            Guard.ArgumentNotNull(address, nameof(address));
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));

            if (readParentOwnerLogin == null)
            {
                readParentOwnerLogin = new Query()
                    .Repository(owner: Var(nameof(owner)), name: Var(nameof(name)))
                    .Select(r => r.Parent != null ? Tuple.Create(r.Parent.Owner.Login, r.Parent.Name) : null)
                    .Compile();
            }

            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
            };

            var graphql = await graphqlFactory.CreateConnection(address);
            var result = await graphql.Run(readParentOwnerLogin, vars);
            return result != null ? (result.Item1, result.Item2) : ((string, string)?)null;
        }
    }
}
