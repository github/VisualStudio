using System;
using System.Reactive;
using System.Windows.Media.Imaging;
using Octokit;

namespace GitHub.Services
{
    public interface IHostAvatarProvider
    {
        IAvatarProvider Get(string gitHubBaseUrl);
    }

    public interface IAvatarProvider
    {
        BitmapImage DefaultUserBitmapImage { get; }
        BitmapImage DefaultOrgBitmapImage { get; }
        IObservable<BitmapSource> GetAvatar(Account apiAccount);
        IObservable<Unit> InvalidateAvatar(Account apiAccount);
        IObservable<BitmapSource> GetAvatar(string email);
    }
}
