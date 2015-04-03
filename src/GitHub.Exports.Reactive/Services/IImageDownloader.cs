using System;

namespace GitHub.Services
{
    public interface IImageDownloader
    {
        IObservable<byte[]> DownloadImageBytes(Uri imageUri);
    }
}   