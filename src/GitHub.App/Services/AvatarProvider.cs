using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Akavache;
using GitHub.Caches;
using GitHub.Models;
using NullGuard;

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

            Uri avatarUrl;
            Uri.TryCreate(apiAccount.AvatarUrl, UriKind.Absolute, out avatarUrl);
            Debug.Assert(avatarUrl != null, "Cannot have a null avatar url");

            return imageCache.GetImage(avatarUrl)
                .Catch<BitmapSource, Exception>(_ => Observable.Return(DefaultAvatar(apiAccount)));
        }
            
        public IObservable<Unit> InvalidateAvatar([AllowNull] IAvatarContainer apiAccount)
        {
            return String.IsNullOrWhiteSpace(apiAccount?.Login)
                ? Observable.Return(Unit.Default)
                : cache.Invalidate(apiAccount.Login);
        }

        BitmapImage DefaultAvatar(IAvatarContainer apiAccount)
        {
            return apiAccount.IsUser ? DefaultUserBitmapImage : DefaultOrgBitmapImage;
        }

        protected virtual void Dispose(bool disposing)
        {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}