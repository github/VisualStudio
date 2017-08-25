using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Akavache;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using NLog;

namespace GitHub.Caches
{
    /// <summary>
    /// An observable wrapper around a <see cref="IKeychain"/>.
    /// </summary>
    /// <remarks>
    /// There are some older classes such as <see cref="RepositoryHost"/> that want an observable
    /// interface for reading credentials. They should be rewritten to use `Task`s but for the
    /// moment this class serves as an adapter for those clients.
    /// </remarks>
    [Export(typeof(IObservableKeychainAdapter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class ObservableKeychainAdapter : IObservableKeychainAdapter
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IKeychain keychain;

        static readonly LoginInfo empty = new LoginInfo("", "");

        [ImportingConstructor]
        public ObservableKeychainAdapter(IKeychain keychain)
        {
            this.keychain = keychain;
        }

        public static LoginInfo EmptyLoginInfo
        {
            get { return empty; }
        }

        public IObservable<LoginInfo> GetLoginAsync(HostAddress hostAddress)
        {
            return keychain.Load(hostAddress)
                .ToObservable()
                .Select(x => new LoginInfo(x.Item1, x.Item2));
        }

        public IObservable<Unit> SaveLogin(string user, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotEmptyString(user, nameof(user));
            Guard.ArgumentNotEmptyString(password, nameof(password));

            return keychain.Save(user, password, hostAddress).ToObservable();
        }

        public IObservable<Unit> EraseLogin(HostAddress hostAddress)
        {
            log.Info(CultureInfo.CurrentCulture, "Erasing the git credential cache for host '{0}'",
                hostAddress.CredentialCacheKeyHost);

            return keychain.Delete(hostAddress).ToObservable();
        }

        public IObservable<Unit> Flush() => Observable.Return(Unit.Default);

        public void Dispose()
        {}
    }
}
