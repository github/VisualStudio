using System;
using System.Reactive;
using System.Windows.Media.Imaging;
using Akavache;
using GitHub.Caches;
using GitHub.Factories;
using GitHub.Services;
using NSubstitute;

namespace UnitTests.Helpers
{
    public class TestImageCache : IImageCache
    {
        IImageCache cacheDelegate;
        public TestImageCache()
        {
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(new InMemoryBlobCache());
            var environment = new Rothko.Environment();
            var imageDownloader = Substitute.For<IImageDownloader>();

            cacheDelegate = new ImageCache(cacheFactory, environment, new Lazy<IImageDownloader>(() => imageDownloader));
        }

        public void Dispose()
        {
            cacheDelegate.Dispose();
        }

        public IObservable<BitmapSource> GetImage(Uri url)
        {
            return cacheDelegate.GetImage(url);
        }

        public IObservable<Unit> Invalidate(Uri url)
        {
            return cacheDelegate.Invalidate(url);
        }

        public IObservable<Unit> SeedImage(Uri url, BitmapSource image, DateTimeOffset expiration)
        {
            return cacheDelegate.SeedImage(url, image, expiration);
        }
    }
}
