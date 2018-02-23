using System;
using System.Windows.Media.Imaging;
using GitHub.Services;
using GitHub.ViewModels;

namespace GitHub.SampleData
{
    public class ActorViewModelDesigner : IActorViewModel
    {
        public ActorViewModelDesigner()
        {
            Avatar = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");
        }

        public BitmapSource Avatar { get; set; }
        public string Login { get; set; }
    }
}
