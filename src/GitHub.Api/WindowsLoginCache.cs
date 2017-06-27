using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Authentication.CredentialManagement;
using GitHub.Extensions;
using GitHub.Primitives;

namespace GitHub.Api
{
    /// <summary>
    /// A login cache that stores logins in the windows credential cache.
    /// </summary>
    [Export(typeof(ILoginCache))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WindowsLoginCache : ILoginCache
    {
        /// <inheritdoc/>
        public Task<Tuple<string, string>> GetLogin(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            using (var credential = new Credential())
            {
                credential.Target = keyHost;
                credential.Type = CredentialType.Generic;
                if (credential.Load())
                    return Task.FromResult(Tuple.Create(credential.Username, credential.Password));
            }

            return Task.FromResult(Tuple.Create<string, string>(null, null));
        }

        /// <inheritdoc/>
        public Task SaveLogin(string userName, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotEmptyString(userName, nameof(userName));
            Guard.ArgumentNotEmptyString(password, nameof(password));
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            using (var credential = new Credential(userName, password, keyHost))
            {
                credential.Save();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task EraseLogin(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            using (var credential = new Credential())
            {
                credential.Target = keyHost;
                credential.Type = CredentialType.Generic;
                credential.Delete();
            }

            return Task.CompletedTask;
        }

        static string GetKeyHost(string key)
        {
            key = FormatKey(key);
            if (key.StartsWith("git:", StringComparison.Ordinal))
                key = key.Substring("git:".Length);
            if (!key.EndsWith("/", StringComparison.Ordinal))
                key += '/';
            return key;
        }

        static string FormatKey(string key)
        {
            if (key.StartsWith("login:", StringComparison.Ordinal))
                key = key.Substring("login:".Length);
            return key;
        }
    }
}
