using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Security;
using Akavache;
using GitHub.Caches;
using GitHub.Extensions;
using GitHub.Primitives;
using LibGit2Sharp;

namespace GitHub.Services
{
    [Export(typeof(IGitHubCredentialProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class GitHubCredentialProvider : IGitHubCredentialProvider
    {
        readonly ISecureBlobCache secureCache = null;

        [ImportingConstructor]
        public GitHubCredentialProvider(ISharedCache sharedCache)
        {
            Guard.ArgumentNotNull(sharedCache, nameof(sharedCache));

            secureCache = sharedCache.Secure;
        }

        /// <summary>
        /// This is a callback from libgit2
        /// </summary>
        /// <returns></returns>
        public Credentials HandleCredentials(string url, string username, SupportedCredentialTypes types)
        {
            if (url == null)
                return null; // wondering if we should return DefaultCredentials instead

            var host = HostAddress.Create(url);
            return secureCache.GetObject<Tuple<string, SecureString>>("login:" + host.CredentialCacheKeyHost)
                .Select(CreateCredentials)
                .Catch(Observable.Return<Credentials>(null))
                .Wait();
        }

        static Credentials CreateCredentials(Tuple<string, SecureString> data)
        {
            Guard.ArgumentNotNull(data, nameof(data));

            return new SecureUsernamePasswordCredentials
            {
                Username = data.Item1,
                Password = data.Item2
            };
        }
    }
}