using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;
using GitHub.Caches;
using GitHub.Extensions.Reactive;
using GitHub.Models;

namespace GitHub.Services
{
    public class GravatarProvider : IAvatarProvider
    {
        const string gravatarBaseUrl = "https://secure.gravatar.com/avatar";
        readonly IImageCache imageCache;

        public GravatarProvider(IImageCache imageCache)
        {
            this.imageCache = imageCache;

            DefaultUserBitmapImage = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");
            DefaultOrgBitmapImage = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");
        }

        public BitmapImage DefaultUserBitmapImage { get; private set; }

        public BitmapImage DefaultOrgBitmapImage { get; private set; }

        public IObservable<BitmapSource> GetAvatar(IAvatarContainer apiAccount)
        {
            throw new InvalidOperationException("Not a valid operation for non-GitHub repositories");
        }

        public IObservable<Unit> InvalidateAvatar(IAvatarContainer apiAccount)
        {
            throw new InvalidOperationException("Not a valid operation for non-GitHub repositories");
        }

        public IObservable<BitmapSource> GetAvatar(string email)
        {
            var url = GravatarUrlForEmail(email);

            return imageCache.GetImage(url)
                .Catch<BitmapSource, KeyNotFoundException>(ex =>
                {
                    // Let's wait one day before trying this again.
                    return imageCache.SeedImage(url, DefaultUserBitmapImage, DateTimeOffset.Now.AddDays(1))
                        .ContinueAfter(() => Observable.Return(DefaultUserBitmapImage));
                });
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Gravatar expects hash of lowercased email")]
        public static Uri GravatarUrlForEmail(string email, int size = 140)
        {
            if (String.IsNullOrEmpty(email))
            {
                return null;
            }

            var md5 = GetMd5Hash(email.ToLowerInvariant());

            return new Uri(
                String.Format(CultureInfo.InvariantCulture, "{0}/{1}?s={2}&d=404", gravatarBaseUrl, md5, size),
                UriKind.Absolute);
        }

        static string GetMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = md5.ComputeHash(bytes);

                return string.Join("", hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        public void Dispose()
        {
            // Let the AvatarProvider dispose the image cache.
            GC.SuppressFinalize(this);
        }
    }
}