using System;
using System.Reactive;
using System.Windows.Media.Imaging;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IAvatarProvider : IDisposable
    {
        BitmapImage DefaultUserBitmapImage { get; }
        BitmapImage DefaultOrgBitmapImage { get; }
        IObservable<BitmapSource> GetAvatar(IAvatarContainer account);
        IObservable<BitmapSource> GetAvatar(string avatarUri);
        IObservable<Unit> InvalidateAvatar(IAvatarContainer account);
    }
}
