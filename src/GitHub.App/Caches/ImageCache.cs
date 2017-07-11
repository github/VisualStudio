using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Media.Imaging;
using Akavache;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Services;
using Rothko;

namespace GitHub.Caches
{
    [Export(typeof(IImageCache))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class ImageCache : IImageCache
    {
        static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public const string ImageCacheFileName = "images.cache.db";
        readonly IObservable<IBlobCache> cacheFactory;
        readonly Lazy<IImageDownloader> imageDownloader;

        readonly SerializedObservableProvider<Uri, BitmapSource> serializedGet;

        readonly Random random = new Random();

        [ImportingConstructor]
        public ImageCache(
            IBlobCacheFactory cacheFactory,
            IEnvironment environment,
            Lazy<IImageDownloader> imageDownloader)
            : this(CreateCache(cacheFactory, environment), imageDownloader)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCache"/> class.
        /// Intended for use from tests, not for public consumption.
        /// </summary>
        internal ImageCache(IBlobCache cache, Lazy<IImageDownloader> imageDownloader)
            : this(Observable.Return(cache), imageDownloader)
        {
        }

        ImageCache(IObservable<IBlobCache> cacheFactory, Lazy<IImageDownloader> imageDownloader)
        {
            this.cacheFactory = cacheFactory;
            this.imageDownloader = imageDownloader;
            serializedGet = new SerializedObservableProvider<Uri, BitmapSource>(GetImageImpl, UriComparer.Default);
        }

        public IObservable<BitmapSource> GetImage(Uri url)
        {
            return serializedGet.Get(url);
        }

        IObservable<BitmapSource> GetImageImpl(Uri url)
        {
            return Observable.Defer(() =>
            {
                var maxCacheDuration = TimeSpan.FromDays(30);
                TimeSpan refreshInterval;

                // Random is not thread safe and multi-threaded access can lead to 
                // a race with the outcome that it will only ever return 0.
                lock (random)
                {
                    // Between 1 and 2 days.
                    refreshInterval = TimeSpan.FromSeconds(random.Next(86400, 172800));
                }

                var ret = new ReplaySubject<BitmapSource>(1);

                cacheFactory.SelectMany(c => c.GetAndRefresh(GetCacheKey(url),() => DownloadImage(url),
                        refreshInterval,
                        maxCacheDuration))
                    .SelectMany(x => LoadImage(x, url))
                    .Catch<BitmapSource, Exception>(ex =>
                        Observable.Throw<BitmapSource>(new KeyNotFoundException("Could not load image: " + url, ex)))
                    .ErrorIfEmpty(new KeyNotFoundException("Could not load image: " + url))
                    .Multicast(ret)
                    .Connect();

                // If GetAndRefresh finds that the cache item is stale it produce the
                // stale value and then fetch and produce a fresh one. It can thus produce
                // 1, 2 or no values (if cache miss and fetch fails). While I'd ideally want
                // to expose that through this method so that images in the UI get updated
                // as soon as we have a fresh value this method has historically only produced
                // one value so in an effort to reduce scope I'm keeping it that way. We
                // unfortunately still need to maintain the subscription to GetAndRefresh though
                // so that we don't cancel the refresh as soon as we get the stale object.
                return ret.Take(1);
            });
        }

        public IObservable<Unit> SeedImage(Uri url, byte[] imageBytes, DateTimeOffset expiration)
        {
            return cacheFactory.SelectMany(c => c.Insert(GetCacheKey(url), imageBytes, expiration));
        }

        public IObservable<Unit> SeedImage(Uri url, BitmapSource image, DateTimeOffset expiration)
        {
            return SeedImage(url, GetBytesFromBitmapImage(image), expiration);
        }

        public IObservable<Unit> Invalidate(Uri url)
        {
            return cacheFactory.SelectMany(c => c.Invalidate(GetCacheKey(url)));
        }

        public static byte[] GetBytesFromBitmapImage(BitmapSource imageSource)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        IObservable<byte[]> DownloadImage(Uri url)
        {
            return ImageDownloader.DownloadImageBytes(url)
                .SelectMany(x => x == null
                    ? Observable.Empty<byte[]>()
                    : Observable.Return(x));
        }

        IImageDownloader ImageDownloader { get { return imageDownloader.Value; } }

        static IObservable<IBlobCache> CreateCache(IBlobCacheFactory cacheFactory, IEnvironment environment)
        {
            return Observable.Defer(() =>
            {
                var imageCacheFilePath = Path.Combine(environment.GetLocalGitHubApplicationDataPath(), ImageCacheFileName);
                var blobCache = cacheFactory.CreateBlobCache(imageCacheFilePath);

                return VacuumIfNecessary(blobCache).ContinueAfter(() => Observable.Return(blobCache));
            })
            .PublishLast()
            .RefCount();
        }

        static IObservable<Unit> VacuumIfNecessary(IBlobCache blobCache, bool force = false)
        {
            const string key = "__Vacuumed";
            var vauumInterval = TimeSpan.FromDays(30);

            return Observable.Defer(() => blobCache.GetCreatedAt(key))
                .Where(lastVacuum => force || !lastVacuum.HasValue || blobCache.Scheduler.Now - lastVacuum.Value > vauumInterval)
                .SelectMany(Observable.Defer(blobCache.Vacuum))
                .SelectMany(Observable.Defer(() => blobCache.Insert(key, new byte[] { 1 })))
                .Catch<Unit, Exception>(ex =>
                {
                    log.Error("Could not vacuum image cache", ex);
                    return Observable.Return(Unit.Default);
                })
                .AsCompletion();
        }

        IObservable<BitmapSource> LoadImage(byte[] x, Uri url)
        {
            return cacheFactory.SelectMany(cache =>
            {
                return LoadImage(new MemoryStream(x), 32, 32)
                    .Catch<BitmapImage, Exception>(ex =>
                    {
                        // This image is likely corrupt. 
                        // Purge the cache for next time.
                        // Re throw so that the caller can decide 
                        // what the fallback is
                        return cache.Invalidate(GetCacheKey(url))
                            .ContinueAfter(() => Observable.Throw<BitmapImage>(ex));
                    });
            });
        }

        static IObservable<BitmapImage> LoadImage(Stream sourceStream, float? desiredWidth = null, float? desiredHeight = null)
        {
            Guard.ArgumentNotNull(sourceStream, nameof(sourceStream));

            return Observable.Defer(() =>
            {
                using (sourceStream)
                {
                    var bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    if (desiredWidth != null && desiredHeight != null)
                    {
                        bitmapImage.DecodePixelWidth = (int)desiredWidth;
                        bitmapImage.DecodePixelHeight = (int)desiredHeight;
                    }
                    bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = sourceStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return Observable.Return(bitmapImage);
                }
            });
        }

        static string GetCacheKey(Uri url)
        {
            return url.ToString();
        }

        public void Dispose()
        {}

        class UriComparer : IEqualityComparer<Uri>
        {
            public static readonly UriComparer Default = new UriComparer();

            public bool Equals(Uri x, Uri y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                    return false;

                return StringComparer.Ordinal.Equals(x.ToString(), y.ToString());
            }

            public int GetHashCode(Uri obj)
            {
                return StringComparer.Ordinal.GetHashCode(obj.ToString());
            }
        }
    }
}
