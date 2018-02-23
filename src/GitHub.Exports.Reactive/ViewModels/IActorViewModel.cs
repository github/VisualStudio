using System.Windows.Media.Imaging;

namespace GitHub.ViewModels
{
    public interface IActorViewModel
    {
        BitmapSource Avatar { get; }
        string Login { get; }
    }
}