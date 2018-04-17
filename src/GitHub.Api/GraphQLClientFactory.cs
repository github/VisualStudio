using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;

namespace GitHub.Api
{
    /// <summary>
    /// Creates GraphQL <see cref="Octokit.GraphQL.IConnection"/>s for querying the
    /// GitHub GraphQL API.
    /// </summary>
    [Export(typeof(IGraphQLClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GraphQLClientFactory : IGraphQLClientFactory
    {
        readonly IKeychain keychain;
        readonly IProgram program;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLClientFactory"/> class.
        /// </summary>
        /// <param name="keychain">The <see cref="IKeychain"/> to use.</param>
        /// <param name="program">The program details.</param>
        [ImportingConstructor]
        public GraphQLClientFactory(IKeychain keychain, IProgram program)
        {
            this.keychain = keychain;
            this.program = program;
        }

        /// <inheirtdoc/>
        public Task<Octokit.GraphQL.IConnection> CreateConnection(HostAddress address)
        {
            var credentials = new GraphQLKeychainCredentialStore(keychain, address);
            var header = new ProductHeaderValue(program.ProductHeader.Name, program.ProductHeader.Version);
            return Task.FromResult<Octokit.GraphQL.IConnection>(new Connection(header, address.GraphQLUri, credentials));
        }
    }
}
