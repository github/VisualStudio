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
        static ICompiledQuery<string> readParentOwnerLogin;
        readonly IGraphQLClientFactory graphqlFactory;

        [ImportingConstructor]
        public RepositoryService(IGraphQLClientFactory graphqlFactory)
        {
            Guard.ArgumentNotNull(graphqlFactory, nameof(graphqlFactory));

            this.graphqlFactory = graphqlFactory;
        }

        public async Task<string> ReadParentOwnerLogin(HostAddress address, string owner, string name)
        {
            Guard.ArgumentNotNull(address, nameof(address));
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));

            if (readParentOwnerLogin == null)
            {
                readParentOwnerLogin = new Query()
                    .Repository(Var(nameof(owner)), Var(nameof(name)))
                    .Select(r => r.Parent != null ? r.Parent.Owner.Login : null)
                    .Compile();
            }

            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
            };

            var graphql = await graphqlFactory.CreateConnection(address);
            return await graphql.Run(readParentOwnerLogin, vars);
        }
    }
}
