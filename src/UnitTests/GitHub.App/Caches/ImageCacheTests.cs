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
using Xunit;

public class ImageCacheTests
{
    public class TheGetImageBytesMethod : TestBaseClass
    {
        [Fact]
        public async Task RetrievesImageFromCacheAndDoesNotFetchIt()
        {
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");

            //Creating a memory cache preloaded with a single pixel
            var cache = new InMemoryBlobCache();
            await cache.Insert("https://fake/", singlePixel);

            //Creating a cache factory that returns the cache
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);

            //Creating an image downloader that will fail when invoked
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(Args.Uri).Returns(_ => { throw new InvalidOperationException(); });

            //Creating the image cache
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            //Retrieving the image demonstrating that the image downloader is not invoked
            var retrieved = await imageCache.GetImage(new Uri("https://fake/")).FirstAsync();

            //Demonstrating that an image was downloaded and in the expected dimensions
            Assert.NotNull(retrieved);
            Assert.Equal(32, retrieved.PixelWidth);
            Assert.Equal(32, retrieved.PixelHeight);
        }

        [Fact]
        public async Task WhenLoadingFromCacheFailsInvalidatesCacheEntry()
        {
            //Creating a memory cache preloaded with an invalid image
            var cache = new InMemoryBlobCache();
            await cache.Insert("https://fake/", new byte[] { 0, 0, 0 });

            //Creating a cache factory that returns the cache
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);

            //Creating an image downloader that will fail when invoked
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(Args.Uri).Returns(_ => { throw new InvalidOperationException(); });

            //Creating the image cache
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            //Attempting to retrieving the image, catching an exception and returning null if one occurs
            var retrieved = await imageCache
                .GetImage(new Uri("https://fake/"))
                .Catch(Observable.Return<BitmapSource>(null))
                .FirstAsync();

            //Demonstrating the value is null, which means the exception occured
            Assert.Null(retrieved);

            //Demonstrating the item is no longer in the cache
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await cache.Get("https://fake/"));
        }
        
        [Fact]
        public async Task DownloadsImageWhenMissingAndCachesIt()
        {
            //Creating a cache factory that returns and empty in memory cache
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(new InMemoryBlobCache());

            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");
            var imageUri = new Uri("https://example.com/poop.gif");

            //Creating an image downloader that returns a single pixel
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(imageUri).Returns(Observable.Return(singlePixel));

            //Creating the image cache
            var imageCache = new ImageCache(cacheFactory, Substitute.For<Rothko.Environment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            //Getting the image through the cache
            var retrieved = await imageCache.GetImage(imageUri).FirstAsync();

            //Demonstrating the image is retrieved and in the correct dimensions
            Assert.NotNull(retrieved);
            Assert.Equal(32, retrieved.PixelWidth);
            Assert.Equal(32, retrieved.PixelHeight);
        }

        [Fact]
        public async Task ThrowsKeyNotFoundExceptionWhenItemNotInCacheAndImageFetchThrowsException()
        {
            var imageUri = new Uri("https://example.com/poop.gif");

            //Creating a cache factory that returns and empty in memory cache
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(new InMemoryBlobCache());

            //Creating an image downloader that throws an exception
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(imageUri).Returns(Observable.Throw<byte[]>(new InvalidOperationException()));

            //Creating the image cache
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            //Getting the image through the cache
            //Demonstrating that KeyNotFoundException is thrown when the downloader throws an exception
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await
                imageCache.GetImage(imageUri).FirstAsync());
        }

        [Fact]
        public async Task ThrowsKeyNotFoundExceptionWhenItemNotInCacheAndImageFetchReturnsEmpty()
        {
            var imageUri = new Uri("https://example.com/poop.gif");

            //Creating a cache factory that returns and empty in memory cache
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(new InMemoryBlobCache());

            //Creating an image downloader that returns no image
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(imageUri).Returns(Observable.Empty<byte[]>());

            //Creating the image cache
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            //Getting the image through the cache
            //Demonstrating that KeyNotFoundException is thrown when the downloader returns no image
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await
                imageCache.GetImage(imageUri).FirstAsync());
        }

        [Fact]
        public void OnlyDownloadsAndDecodesOnceForConcurrentOperations()
        {
            //Creating a cache for concurrent testing
            var cache = new InMemoryBlobCache(Scheduler.Immediate);

            //Creating a cache factory that returns that cache
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);

            var subj = new Subject<byte[]>();

            //Creating an image downloader that first returns test observable
            var imageDownloader = Substitute.For<IImageDownloader>();
            imageDownloader.DownloadImageBytes(Args.Uri).Returns(subj, Observable.Throw<byte[]>(new InvalidOperationException()));

            //Creating the image cache
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => imageDownloader));

            var uri = new Uri("https://github.com/foo.png");

            BitmapSource res1 = null;
            BitmapSource res2 = null;
            
            //Initiating two concurrent get image requests, demonstrating the image downloader is only invoked once
            var sub1 = imageCache.GetImage(uri).Subscribe(x => res1 = x);
            var sub2 = imageCache.GetImage(uri).Subscribe(x => res2 = x);

            //Demonstrating the images have not been downloaded yet
            Assert.Null(res1);
            Assert.Null(res2);

            //Providing the test observable with a single pixel image
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");
            subj.OnNext(singlePixel);
            subj.OnCompleted();

            //Demonstrating the image is not null and received twice
            Assert.NotNull(res1);
            Assert.Equal(res1, res2);
        }
    }

    public class TheInvalidateMethod : TestBaseClass
    {
        [Fact]
        public async Task RemovesImageFromCache()
        {
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");

            //Creating a memory cache preloaded with a single pixel
            var cache = new InMemoryBlobCache();
            await cache.Insert("https://fake/", singlePixel);

            //Creating a cache factory that returns the cache
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);

            //Creating the image cache
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => Substitute.For<IImageDownloader>()));

            //Invalidate the image
            await imageCache.Invalidate(new Uri("https://fake/"));

            //Demonstrating that if the image is requested KeyNotFoundException is thrown
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await cache.Get("https://fake/"));
        }
    }

    public class TheSeedImageMethod : TestBaseClass
    {
        [Fact]
        public async Task AddsImageDirectlyToCache()
        {
            var singlePixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==");

            //Creating an in memory cache object
            var cache = new InMemoryBlobCache();
          
            //Creating a cache factory that returns that cache
            var cacheFactory = Substitute.For<IBlobCacheFactory>();
            cacheFactory.CreateBlobCache(Args.String).Returns(cache);

            //Creating the image cache
            var imageCache = new ImageCache(cacheFactory, Substitute.For<IEnvironment>(), new Lazy<IImageDownloader>(() => Substitute.For<IImageDownloader>()));

            //Seeding the image
            await imageCache.SeedImage(new Uri("https://fake/"), singlePixel, DateTimeOffset.MaxValue);

            //Getting the image through the cache
            var retrieved = await cache.Get("https://fake/");
      
            //Demonstrating that the image is unchanged
            Assert.Equal(singlePixel, retrieved);
        }
    }
}
