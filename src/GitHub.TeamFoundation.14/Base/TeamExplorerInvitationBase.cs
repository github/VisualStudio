using GitHub.Services;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using System;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerInvitationBase : TeamExplorerBase, ITeamExplorerServiceInvitation, INotifyPropertySource
    {
        public static readonly Guid TeamExplorerInvitationSectionGuid = new Guid("8914ac06-d960-4537-8345-cb13c00378d8");

        protected TeamExplorerInvitationBase(IGitHubServiceProvider serviceProvider) : base(serviceProvider)
        {}

        public virtual void Initialize(IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            TEServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Triggers the login flow
        /// </summary>
        public virtual void Connect() {}

        /// <summary>
        /// Starts the sign up process online
        /// </summary>
        public virtual void SignUp() {}


        bool canConnect;
        public bool CanConnect
        {
            get { return canConnect; }
            set { canConnect = value; this.RaisePropertyChange(); }
        }

        bool canSignUp;
        public bool CanSignUp
        {
            get { return canSignUp; }
            set { canSignUp = value; this.RaisePropertyChange(); }
        }

        string connectLabel;
        public string ConnectLabel
        {
            get { return connectLabel; }
            set { connectLabel = value; this.RaisePropertyChange(); }
        }

        string description;
        public string Description
        {
            get { return description; }
            set { description = value; this.RaisePropertyChange(); }
        }

        object icon;
        public object Icon
        {
            get { return icon; }
            set { icon = value; this.RaisePropertyChange(); }
        }

        bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; this.RaisePropertyChange(); }
        }

        string name;
        public string Name
        {
            get { return name; }
            set { name = value; this.RaisePropertyChange(); }
        }

        string provider;
        public string Provider
        {
            get { return provider; }
            set { provider = value; this.RaisePropertyChange(); }
        }

        string signUpLabel;
        public string SignUpLabel
        {
            get { return signUpLabel; }
            set { signUpLabel = value; this.RaisePropertyChange(); }
        }
    }
}
