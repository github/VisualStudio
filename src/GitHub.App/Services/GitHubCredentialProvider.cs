using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Security;
using Akavache;
using GitHub.Caches;
using GitHub.Primitives;
using LibGit2Sharp;
using NullGuard;

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
            secureCache = sharedCache.Secure;
        }

        /// <summary>
        /// This is a callback from libgit2
        /// </summary>
        /// <returns></returns>
        public Credentials HandleCredentials([AllowNull]string url, [AllowNull]string username, SupportedCredentialTypes types)
        {
            if (url == null)
                return new DefaultCredentials();

            var host = HostAddress.Create(url);
            return secureCache.GetObject<Tuple<string, SecureString>>("login:" + host.CredentialCacheKeyHost)
                .Select(x => new SecureUsernamePasswordCredentials()
                {
                    Username = x.Item1,
                    Password = x.Item2
                } as Credentials)
                .Catch(Observable.Return(new DefaultCredentials()))
                .Wait();
        }
    }
}