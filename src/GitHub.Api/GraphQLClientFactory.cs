using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using Octokit.GraphQL;

namespace GitHub.Api
{
    [Export(typeof(IGraphQLClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GraphQLClientFactory : IGraphQLClientFactory
    {
        readonly IKeychain keychain;
        readonly IProgram program;

        [ImportingConstructor]
        public GraphQLClientFactory(IKeychain keychain, IProgram program)
        {
            this.keychain = keychain;
            this.program = program;
        }

        public Task<Octokit.GraphQL.Connection> CreateConnection(IConnection connection)
        {
            var credentials = new GraphQLKeychainCredentialStore(keychain, connection.HostAddress);
            var header = new ProductHeaderValue(program.ProductHeader.Name, program.ProductHeader.Version);
            return Task.FromResult(new Connection(header, credentials));
        }
    }
}
