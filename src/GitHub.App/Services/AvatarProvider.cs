using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Media.Imaging;
using Akavache;
using GitHub.Caches;
using GitHub.Helpers;
using GitHub.Models;
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

        [ImportingConstructor]
        public AvatarProvider(ISharedCache sharedCache, IImageCache imageCache)
        {
            cache = sharedCache.LocalMachine;
            this.imageCache = imageCache;

            DefaultUserBitmapImage = ImageHelper.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");
            DefaultOrgBitmapImage = ImageHelper.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");

            // NB: We pick 32 here to be roughly the same size as the list of items
            // in the commit list, to the nearest round number
            userAvatarCache = new MemoizingMRUCache<IAvatarContainer, AsyncSubject<BitmapSource>>(
                (account, _) => GetAvatarForAccount(account), 32);
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

        public IObservable<Unit> InvalidateAvatar(IAvatarContainer apiAccount)
        {
            return apiAccount == null || String.IsNullOrWhiteSpace(apiAccount.Login)
                ? Observable.Return(Unit.Default)
                : cache.Invalidate(apiAccount.Login);
        }

        AsyncSubject<BitmapSource> GetAvatarForAccount(IAvatarContainer account)
        {
            var ret = new AsyncSubject<BitmapSource>();

            imageCache.GetImage(account.AvatarUrl)
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