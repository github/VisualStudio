using System;
using System.Windows.Media.Imaging;
using GitHub.Models;

namespace GitHub.ViewModels
{
    public class ActorViewModel : ViewModelBase, IActorViewModel
    {
        public ActorViewModel(ActorModel model)
        {
            Login = model.Login;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(model.AvatarUrl);
            image.EndInit();
            Avatar = image;
        }

        public BitmapSource Avatar { get; }
        public string Login { get; }
    }
}
