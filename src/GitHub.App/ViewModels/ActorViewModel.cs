using System;
using System.Windows.Media.Imaging;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.ViewModels
{
    public class ActorViewModel : ViewModelBase, IActorViewModel
    {
        const string DefaultAvatar = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";

        public ActorViewModel()
        {
        }

        public ActorViewModel(ActorModel model)
        {
            Login = model?.Login ?? "[unknown]";
            Avatar = model?.AvatarUrl != null ?
                new BitmapImage(new Uri(model.AvatarUrl)) :
                AvatarProvider.CreateBitmapImage(DefaultAvatar);
            AvatarUrl = model?.AvatarUrl ?? DefaultAvatar;
        }

        public BitmapSource Avatar { get; set; }
        public string AvatarUrl { get; set; }
        public string Login { get; set; }
    }
}