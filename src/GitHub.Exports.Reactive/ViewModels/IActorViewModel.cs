using System.Windows.Media.Imaging;

namespace GitHub.ViewModels
{
    public interface IActorViewModel : IViewModel
    {
        BitmapSource Avatar { get; }
        string Login { get; }
    }
}