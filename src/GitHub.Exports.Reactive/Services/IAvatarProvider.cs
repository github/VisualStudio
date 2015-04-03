using System;
using System.Reactive;
using System.Windows.Media.Imaging;
using GitHub.Caches;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IAvatarProvider
    {
        BitmapImage DefaultUserBitmapImage { get; }
        BitmapImage DefaultOrgBitmapImage { get; }
        IObservable<BitmapSource> GetAvatar(IAvatarContainer account);
        IObservable<Unit> InvalidateAvatar(IAvatarContainer account);
    }
}
