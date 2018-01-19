using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Akavache;
using GitHub.Caches;
using GitHub.Factories;
using GitHub.Services;
using NSubstitute;
using Rothko;
using NUnit.Framework;

public class ImageCacheTests
{
    public class TheGetImageBytesMethod : TestBaseClass
    {
        [Test]
        public async Task RetrievesImageFromCacheAndDoesNotFetchIt()
        {
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");
            var cache = new InMemoryBlobCache();
            await cache.Insert("https://fake/", singlePixel);
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(Args.Uri).Returns(_ => { throw new InvalidOperationException(); });
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            var retrieved = await imageCache.GetImage(new Uri("https://fake/")).FirstAsync();

            Assert.NotNull(retrieved);
            Assert.That(32, Is.EqualTo(retrieved.PixelWidth));
            Assert.That(32, Is.EqualTo(retrieved.PixelHeight));
        }

        [Test]
        public async Task WhenLoadingFromCacheFailsInvalidatesCacheEntry()
        {
            var cache = new InMemoryBlobCache();
            await cache.Insert("https://fake/", new byte[] { 0, 0, 0 });
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(Args.Uri).Returns(_ => { throw new InvalidOperationException(); });
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            var retrieved = await imageCache
                .GetImage(new Uri("https://fake/"))
                .Catch(Observable.Return<BitmapSource>(null))
                .FirstAsync();

            Assert.That(retrieved, Is.Null);
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await cache.Get("https://fake/"));
        }
        
        [Test]
        public async Task DownloadsImageWhenMissingAndCachesIt()
        {
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");

            var imageUri = new Uri("https://example.com/poop.gif");
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(new InMemoryBlobCache());
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(imageUri).Returns(Observable.Return(singlePixel));
            var imageCache = new ImageCache(cacheFactory, Substitute.For<Rothko.Environment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            var retrieved = await imageCache.GetImage(imageUri).FirstAsync();

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(32, Is.EqualTo(retrieved.PixelWidth));
            Assert.That(32, Is.EqualTo(retrieved.PixelHeight));
        }

        [Test]
        public async Task ThrowsKeyNotFoundExceptionWhenItemNotInCacheAndImageFetchThrowsException()
        {
            var imageUri = new Uri("https://example.com/poop.gif");
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(new InMemoryBlobCache());
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(imageUri).Returns(Observable.Throw<byte[]>(new InvalidOperationException()));

            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await
                imageCache.GetImage(imageUri).FirstAsync());
        }

        [Test]
        public async Task ThrowsKeyNotFoundExceptionWhenItemNotInCacheAndImageFetchReturnsEmpty()
        {
            var imageUri = new Uri("https://example.com/poop.gif");
            var cache = new InMemoryBlobCache();
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(imageUri).Returns(Observable.Empty<byte[]>());

            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await
                     imageCache.GetImage(imageUri).FirstAsync());
        }

        [Test]
        public void OnlyDownloadsAndDecodesOnceForConcurrentOperations()
        {
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");
            var subj = new Subject<byte[]>();

            var cache = new InMemoryBlobCache(Scheduler.Immediate);
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(Args.Uri).Returns(subj, Observable.Throw<byte[]>(new InvalidOperationException()));
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));
            var uri = new Uri("https://github.com/foo.png");

            BitmapSource res1 = null;
            BitmapSource res2 = null;

            var sub1 = imageCache.GetImage(uri).Subscribe(x => res1 = x);
            var sub2 = imageCache.GetImage(uri).Subscribe(x => res2 = x);

            Assert.That(res1, Is.Null);
            Assert.That(res2, Is.Null);

            subj.OnNext(singlePixel);
            subj.OnCompleted();

            Assert.That(res1, Is.Not.Null);
            Assert.That(res1, Is.EqualTo(res2));
        }
    }

    public class TheInvalidateMethod : TestBaseClass
    {
        [Test]
        public async Task RemovesImageFromCache()
        {
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");
            var cache = new InMemoryBlobCache();
            await cache.Insert("https://fake/", singlePixel);
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => Substitute.For<IImageDownloader>()));

            await imageCache.Invalidate(new Uri("https://fake/"));

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await cache.Get("https://fake/"));
        }
    }

    public class TheSeedImageMethod : TestBaseClass
    {
        [Test]
        public async Task AddsImageDirectlyToCache()
        {
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");
            var cache = new InMemoryBlobCache();
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => Substitute.For<IImageDownloader>()));

            await imageCache.SeedImage(new Uri("https://fake/"), singlePixel, DateTimeOffset.MaxValue);

            var retrieved = await cache.Get("https://fake/");
            Assert.That(singlePixel, Is.EqualTo(retrieved));
        }
    }
}
