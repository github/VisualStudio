using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Primitives;
using Serilog;
using Serilog.Core;

namespace GitHub.Caches
{
    [Export(typeof(ILoginCache))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class LoginCache : ILoginCache
    {
        static readonly ILogger log = LogManager.ForContext<LoginCache>();
        readonly ISharedCache cache;

        static readonly LoginInfo empty = new LoginInfo("", "");

        [ImportingConstructor]
        public LoginCache(ISharedCache cache)
        {
            this.cache = cache;
        }

        public static LoginInfo EmptyLoginInfo
        {
            get { return empty; }
        }

        public IObservable<LoginInfo> GetLoginAsync(HostAddress hostAddress)
        {
            return cache.Secure.GetLoginAsync(hostAddress.CredentialCacheKeyHost)
                .Catch<LoginInfo, Exception>(e => Observable.Return(empty));
        }

        public IObservable<Unit> SaveLogin(string user, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotEmptyString(user, nameof(user));
            Guard.ArgumentNotEmptyString(password, nameof(password));

            return cache.Secure.SaveLogin(user, password, hostAddress.CredentialCacheKeyHost);
        }

        public IObservable<Unit> EraseLogin(HostAddress hostAddress)
        {
            log.Information("Erasing the git credential cache for host {CredentialCacheKeyHost}", hostAddress.CredentialCacheKeyHost);
            return cache.Secure.EraseLogin(hostAddress.CredentialCacheKeyHost);
        }

        public IObservable<Unit> Flush()
        {
            log.Information("Flushing the login cache");
            return cache.Secure.Flush();
        }

        public void Dispose()
        {}
    }
}
