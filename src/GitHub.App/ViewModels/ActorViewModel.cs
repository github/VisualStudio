using System;
using System.Windows.Media.Imaging;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Serilog;

namespace GitHub.ViewModels
{
    public class ActorViewModel : ViewModelBase, IActorViewModel
    {
        const string DefaultAvatar = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";
        static readonly ILogger log = LogManager.ForContext<ActorViewModel>();

        public ActorViewModel()
        {
        }

        public ActorViewModel(ActorModel model)
        {
            Login = model?.Login ?? "[unknown]";

            if (model?.AvatarUrl != null)
            {
                try
                {
                    var uri = new Uri(model.AvatarUrl);

                    // Image requests to enterprise hosts over https always fail currently,
                    // so just display the default avatar. See #1547.
                    if (uri.Scheme != "https" ||
                        uri.Host.EndsWith("githubusercontent.com", StringComparison.OrdinalIgnoreCase))
                    {
                        AvatarUrl = model.AvatarUrl;
                        Avatar = new BitmapImage(uri);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Invalid avatar URL");
                }
            }

            if (AvatarUrl == null)
            {
                Avatar = AvatarProvider.CreateBitmapImage(DefaultAvatar);
                AvatarUrl = DefaultAvatar;
            }
        }

        public BitmapSource Avatar { get; set; }
        public string AvatarUrl { get; set; }
        public string Login { get; set; }
    }
}