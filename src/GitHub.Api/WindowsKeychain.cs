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

            var key = GetKey(hostAddress.CredentialCacheKeyHost);
            var keyGit = GetKeyGit(hostAddress.CredentialCacheKeyHost);
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);
            Tuple<string, string> result = null;

            // Visual Studio requires two credentials, keyed as "{hostname}" (e.g. "https://github.com/") and
            // "git:{hostname}" (e.g. "git:https://github.com"). We have a problem in that these credentials can
            // potentially be overwritten by other applications, so we store an extra "master" key as
            // "GitHub for Visual Studio - {hostname}". Whenever we read the credentials we overwrite the other
            // two keys with the value from the master key. Older versions of GHfVS did not store this master key
            // so if it does not exist, try to get the value from the "{hostname}" key.
            using (var credential = Credential.Load(key))
            using (var credentialGit = Credential.Load(keyGit))
            using (var credentialHost = Credential.Load(keyHost))
            {
                if (credential != null)
                {
                    result = Tuple.Create(credential.Username, credential.Password);
                }
                else if (credentialHost != null)
                {
                    result = Tuple.Create(credentialHost.Username, credentialHost.Password);
                }

                if (result != null)
                {
                    Save(result.Item1, result.Item2, hostAddress);
                }
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task Save(string userName, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotEmptyString(userName, nameof(userName));
            Guard.ArgumentNotEmptyString(password, nameof(password));
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var key = GetKey(hostAddress.CredentialCacheKeyHost);
            var keyGit = GetKeyGit(hostAddress.CredentialCacheKeyHost);
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            Credential.Save(key, userName, password);
            Credential.Save(keyGit, userName, password);
            Credential.Save(keyHost, userName, password);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task Delete(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var key = GetKey(hostAddress.CredentialCacheKeyHost);
            var keyGit = GetKeyGit(hostAddress.CredentialCacheKeyHost);
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            Credential.Delete(key);
            Credential.Delete(keyGit);
            Credential.Delete(keyHost);

            return Task.CompletedTask;
        }

        static string GetKey(string key)
        {
            key = FormatKey(key);
            if (key.StartsWith("git:", StringComparison.Ordinal))
                key = key.Substring("git:".Length);
            if (!key.EndsWith("/", StringComparison.Ordinal))
                key += '/';
            return "GitHub for Visual Studio - " + key;
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
