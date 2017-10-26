using System.Threading.Tasks;
using GitHub.Primitives;
using Octokit;
using System;
using GitHub.Authentication.CredentialManagement;

namespace GitHub.Api
{
    public class SimpleCredentialStore : ICredentialStore
    {
        readonly HostAddress hostAddress;

        public SimpleCredentialStore(HostAddress hostAddress)
        {
            this.hostAddress = hostAddress;
        }

        public Task<SecureCredential> GetSecureCredentials()
        {
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);
            using (var credential = new Credential())
            {
                credential.Target = keyHost;
                credential.Type = CredentialType.Generic;
                if (credential.Load())
                    return Task.FromResult(new SecureCredential(credential.Username, credential.SecurePassword));
            }
            return Task.FromResult(SecureCredential.Anonymous);
        }

        public Task<Credentials> GetCredentials()
        {
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);
            using (var credential = new Credential())
            {
                credential.Target = keyHost;
                credential.Type = CredentialType.Generic;
                if (credential.Load())
                    return Task.FromResult(new Credentials(credential.Username, credential.Password));
            }
            return Task.FromResult(Credentials.Anonymous);
        }

        public static Task<bool> RemoveCredentials(string key)
        {
            var keyGit = GetKeyGit(key);
            if (!DeleteKey(keyGit))
                return Task.FromResult(false);

            var keyHost = GetKeyHost(key);
            if (!DeleteKey(keyHost))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        static bool DeleteKey(string key)
        {
            using (var credential = new Credential())
            {
                credential.Target = key;
                if (!credential.Load())
                    return false;
                return credential.Delete();
            }
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
    }
}
