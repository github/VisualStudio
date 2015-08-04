using System;
using System.Reactive;
using System.Windows.Media.Imaging;

namespace GitHub.Caches
{
    public interface IImageCache : IDisposable
    {
        /// <summary>
        /// Retrieves the image from the cache. If it's missing, it'll download it and cache it.
        /// </summary>
        /// <param name="url">The image Uri to fetch.</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if an image is missing and could not be fetched.</exception>
        IObservable<BitmapSource> GetImage(Uri url);

        /// <summary>
        /// This is useful when you need placeholder images.
        /// </summary>
        IObservable<Unit> SeedImage(Uri url, BitmapSource image, DateTimeOffset expiration);

        IObservable<Unit> Invalidate(Uri url);
    }
}