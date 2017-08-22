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
        public async Task<Tuple<string, string>> GetLogin(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var creds = await SimpleCredentialStore.GetCredentials(hostAddress);
            return Tuple.Create(creds.Login, creds.Password);
        }

        /// <inheritdoc/>
        public async Task<bool> SaveLogin(string userName, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotEmptyString(userName, nameof(userName));
            Guard.ArgumentNotEmptyString(password, nameof(password));
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            return await SimpleCredentialStore.SaveCredentials(hostAddress, new Octokit.Credentials(userName, password));
        }

        /// <inheritdoc/>
        public async Task<bool> EraseLogin(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            return await SimpleCredentialStore.RemoveCredentials(hostAddress);
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
