using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Authentication.CredentialManagement;
using GitHub.Extensions;
using GitHub.Primitives;

namespace GitHub.Api
{
    /// <summary>
    /// A keychain that stores logins in the windows credential store.
    /// </summary>
    [Export(typeof(IKeychain))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WindowsKeychain : IKeychain
    {
        /// <inheritdoc/>
        public Task<Tuple<string, string>> Load(HostAddress hostAddress)
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
        public Task Save(string userName, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotEmptyString(userName, nameof(userName));
            Guard.ArgumentNotEmptyString(password, nameof(password));
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var keyGit = GetKeyGit(hostAddress.CredentialCacheKeyHost);
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            using (var credential = new Credential(userName, password, keyGit))
            {
                credential.Save();
            }

            using (var credential = new Credential(userName, password, keyHost))
            {
                credential.Save();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task Delete(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var keyGit = GetKeyGit(hostAddress.CredentialCacheKeyHost);
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            using (var credential = new Credential())
            {
                credential.Target = keyGit;
                credential.Type = CredentialType.Generic;
                credential.Delete();
            }

            using (var credential = new Credential())
            {
                credential.Target = keyHost;
                credential.Type = CredentialType.Generic;
                credential.Delete();
            }

            return Task.CompletedTask;
        }

        static string GetKeyGit(string key)
        {
            key = FormatKey(key);
            // it appears this is how MS expects the host key
            if (!key.StartsWith("git:", StringComparison.Ordinal))
                key = "git:" + key;
            if (key.EndsWith("/", StringComparison.Ordinal))
                key = key.Substring(0, key.Length - 1);
            return key;
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
