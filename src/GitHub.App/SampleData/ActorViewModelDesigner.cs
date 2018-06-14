using System;
using System.Windows.Media.Imaging;
using GitHub.Services;
using GitHub.ViewModels;

namespace GitHub.SampleData
{
    public class ActorViewModelDesigner : ViewModelBase, IActorViewModel
    {
        public ActorViewModelDesigner()
        {
            AvatarUrl = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";
            Avatar = AvatarProvider.CreateBitmapImage(AvatarUrl);
        }

        public ActorViewModelDesigner(string login)
            : this()
        {
            Login = login;
        }

        public BitmapSource Avatar { get; }
        public string AvatarUrl { get; }
        public string Login { get; set; }
    }
}
