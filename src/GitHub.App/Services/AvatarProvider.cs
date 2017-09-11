using System;
using System.IO.Packaging;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using Akavache;
using GitHub.Caches;
using GitHub.Infrastructure;
using GitHub.Extensions;
using GitHub.Models;
using System.Windows;

namespace GitHub.Services
{
    [Export(typeof(IAvatarProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AvatarProvider : IAvatarProvider
    {
        readonly IImageCache imageCache;
        readonly IBlobCache cache;

        public BitmapImage DefaultUserBitmapImage { get; private set; }
        public BitmapImage DefaultOrgBitmapImage { get; private set; }

        static AvatarProvider()
        {
            // Calling `Application.Current` will install pack URI scheme via Application.cctor.
            // This is needed when unit testing for the pack:// URL format to be understood.
            if (Application.Current != null) { }
        }

        [ImportingConstructor]
        public AvatarProvider(ISharedCache sharedCache, IImageCache imageCache)
        {
            Guard.ArgumentNotNull(sharedCache, nameof(sharedCache));
            Guard.ArgumentNotNull(imageCache, nameof(imageCache));

            cache = sharedCache.LocalMachine;
            this.imageCache = imageCache;

            DefaultUserBitmapImage = CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");
            DefaultOrgBitmapImage = CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");
        }

        public static BitmapImage CreateBitmapImage(string packUrl)
        {
            Guard.ArgumentNotEmptyString(packUrl, nameof(packUrl));

            var bitmap = new BitmapImage(new Uri(packUrl));
            bitmap.Freeze();
            return bitmap;
        }

        public IObservable<BitmapSource> GetAvatar(IAvatarContainer apiAccount)
        {
            Guard.ArgumentNotNull(apiAccount, nameof(apiAccount));

            if (apiAccount.AvatarUrl == null)
            {
                return Observable.Return(DefaultAvatar(apiAccount));
            }

            Uri avatarUrl;
            Uri.TryCreate(apiAccount.AvatarUrl, UriKind.Absolute, out avatarUrl);
            Log.Assert(avatarUrl != null, "Cannot have a null avatar url");

            return imageCache.GetImage(avatarUrl)
                .Catch<BitmapSource, Exception>(_ => Observable.Return(DefaultAvatar(apiAccount)));
        }

        public IObservable<Unit> InvalidateAvatar(IAvatarContainer apiAccount)
        {
            return String.IsNullOrWhiteSpace(apiAccount?.Login)
                ? Observable.Return(Unit.Default)
                : cache.Invalidate(apiAccount.Login);
        }

        BitmapImage DefaultAvatar(IAvatarContainer apiAccount)
        {
            Guard.ArgumentNotNull(apiAccount, nameof(apiAccount));

            return apiAccount.IsUser ? DefaultUserBitmapImage : DefaultOrgBitmapImage;
        }

        protected virtual void Dispose(bool disposing)
        { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}