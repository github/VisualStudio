using System;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    /// <summary>
    /// An Octokit credential store that reads from an <see cref="IKeychain"/>.
    /// </summary>
    public class KeychainCredentialStore : ICredentialStore
    {
        readonly IKeychain keychain;
        readonly HostAddress address;

        public KeychainCredentialStore(IKeychain keychain, HostAddress address)
        {
            Guard.ArgumentNotNull(keychain, nameof(keychain));
            Guard.ArgumentNotNull(address, nameof(keychain));

            this.keychain = keychain;
            this.address = address;
        }

        public async Task<Credentials> GetCredentials()
        {
            var userPass = await keychain.Load(address).ConfigureAwait(false);
            return userPass != null ? new Credentials(userPass.Item1, userPass.Item2) : Credentials.Anonymous;
        }
    }
}
