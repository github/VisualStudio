using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;
using static Octokit.GraphQL.Variable;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryService: IRepositoryService
    {
        static ICompiledQuery<Tuple<string, string>> readParentOwnerLogin;
        static ICompiledQuery<IEnumerable<ProtectedBranch>> queryProtectedBranches;
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
                    .Repository(Var(nameof(owner)), Var(nameof(name)))
                    .Select(r => r.Parent != null ? Tuple.Create(r.Parent.Owner.Login, r.Parent.Name) : null)
                    .Compile();
            }

            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
            };

            var graphql = await graphqlFactory.CreateConnection(address).ConfigureAwait(false);
            var result = await graphql.Run(readParentOwnerLogin, vars).ConfigureAwait(false);
            return result != null ? (result.Item1, result.Item2) : ((string, string)?)null;
        }

        public async Task<IEnumerable<ProtectedBranch>> GetProtectedBranches(HostAddress address, string owner, string name)
        {
            Guard.ArgumentNotNull(address, nameof(address));
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));

            if (queryProtectedBranches == null)
            {
                queryProtectedBranches = new Query()
                    .Repository(Var(nameof(owner)), Var(nameof(name)))
                    .Select(r =>
                        r.ProtectedBranches(null, null, null, null)
                            .AllPages()
                            .Select(branch => new ProtectedBranch
                            {
                                Name = branch.Name,
                                RequiredStatusCheckContexts = branch.RequiredStatusCheckContexts.ToArray()
                            })
                        ).Compile();
            }

            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
            };

            var graphql = await graphqlFactory.CreateConnection(address).ConfigureAwait(false);
            return await graphql.Run(queryProtectedBranches, vars).ConfigureAwait(false);
        }
    }
}
