using System;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Primitives;
using Octokit.GraphQL;

namespace GitHub.Api
{
    /// <summary>
    /// An Octokit.GraphQL credential store that reads from an <see cref="IKeychain"/>.
    /// </summary>
    public class GraphQLKeychainCredentialStore : ICredentialStore
    {
        readonly IKeychain keychain;
        readonly HostAddress address;

        public GraphQLKeychainCredentialStore(IKeychain keychain, HostAddress address)
        {
            Guard.ArgumentNotNull(keychain, nameof(keychain));
            Guard.ArgumentNotNull(address, nameof(keychain));

            this.keychain = keychain;
            this.address = address;
        }

        public async Task<string> GetCredentials(CancellationToken cancellationToken = default)
        {
            var userPass = await keychain.Load(address).ConfigureAwait(false);
            return userPass?.Item2;
        }
    }
}
