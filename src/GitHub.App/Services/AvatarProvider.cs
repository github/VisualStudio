using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media.Imaging;
using Akavache;
using GitHub.Caches;
using GitHub.Helpers;
using GitHub.Models;
using NullGuard;
using Splat;

namespace GitHub.Services
{
    [Export(typeof(IAvatarProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AvatarProvider : IAvatarProvider
    {
        readonly IImageCache imageCache;
        readonly MemoizingMRUCache<IAvatarContainer, AsyncSubject<BitmapSource>> userAvatarCache;
        readonly IBlobCache cache;

        public BitmapImage DefaultUserBitmapImage { get; private set; }
        public BitmapImage DefaultOrgBitmapImage { get; private set; }

        static AvatarProvider()
        {
            // NB: If this isn't explicitly set, WPF will try to guess it via
            // GetEntryAssembly, which in a unit test runner will be null
            // This is needed for the pack:// URL format to be understood.
            if (Application.ResourceAssembly == null)
            {
                Application.ResourceAssembly = typeof(AvatarProvider).Assembly;
            }
        }

        [ImportingConstructor]
        public AvatarProvider(ISharedCache sharedCache, IImageCache imageCache)
        {
            cache = sharedCache.LocalMachine;
            this.imageCache = imageCache;

            DefaultUserBitmapImage = CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");
            DefaultOrgBitmapImage = CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");

            // NB: We pick 32 here to be roughly the same size as the list of items
            // in the commit list, to the nearest round number
            userAvatarCache = new MemoizingMRUCache<IAvatarContainer, AsyncSubject<BitmapSource>>(
                (account, _) => GetAvatarForAccount(account), 32);
        }

        public static BitmapImage CreateBitmapImage(string packUrl)
        {
            var bitmap = new BitmapImage(new Uri(packUrl));
            bitmap.Freeze();
            return bitmap;
        }

        public IObservable<BitmapSource> GetAvatar(IAvatarContainer apiAccount)
        {
            if (apiAccount.AvatarUrl == null)
            {
                return Observable.Return(DefaultAvatar(apiAccount));
            }

            lock (userAvatarCache)
            {
                return userAvatarCache.Get(apiAccount);
            }
        }

        public IObservable<Unit> InvalidateAvatar([AllowNull] IAvatarContainer apiAccount)
        {
            return apiAccount == null || String.IsNullOrWhiteSpace(apiAccount.Login)
                ? Observable.Return(Unit.Default)
                : cache.Invalidate(apiAccount.Login);
        }

        AsyncSubject<BitmapSource> GetAvatarForAccount(IAvatarContainer account)
        {
            var ret = new AsyncSubject<BitmapSource>();

            Uri avatarUrl;
            Uri.TryCreate(account.AvatarUrl, UriKind.Absolute, out avatarUrl);
            Debug.Assert(avatarUrl != null, "Cannot have a null avatar url");

            imageCache.GetImage(avatarUrl)
                .Catch<BitmapSource, Exception>(_ => Observable.Return(DefaultAvatar(account)))
                .Multicast(ret).Connect();

            return ret;
        }

        BitmapImage DefaultAvatar(IAvatarContainer apiAccount)
        {
            return apiAccount.IsUser ? DefaultUserBitmapImage : DefaultOrgBitmapImage;
        }
    }
}