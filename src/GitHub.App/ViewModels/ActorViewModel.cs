using System;
using System.Windows.Media.Imaging;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.ViewModels
{
    public class ActorViewModel : ViewModelBase, IActorViewModel
    {
        const string DefaultAvatar = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";

        public ActorViewModel(ActorModel model)
        {
            Login = model.Login;
            Avatar = model.AvatarUrl != null ? 
                new BitmapImage(new Uri(model.AvatarUrl)) :
                AvatarProvider.CreateBitmapImage(DefaultAvatar);
        }

        public BitmapSource Avatar { get; }
        public string Login { get; }
    }
}
