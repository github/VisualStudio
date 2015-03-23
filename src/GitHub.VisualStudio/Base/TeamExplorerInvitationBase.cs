using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerInvitationBase : TeamExplorerBase, ITeamExplorerServiceInvitation, INotifyPropertySource
    {
        public TeamExplorerInvitationBase()
        {
            canConnect = true;
            canSignUp = true;
        }

        public virtual void Initialize(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
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
        [AllowNull]
        public string ConnectLabel
        {
            [return: AllowNull]
            get { return connectLabel; }
            set { connectLabel = value; this.RaisePropertyChange(); }
        }

        string description;
        [AllowNull]
        public string Description
        {
            [return: AllowNull]
            get { return description; }
            set { description = value; this.RaisePropertyChange(); }
        }

        object icon;
        [AllowNull]
        public object Icon
        {
            [return: AllowNull]
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
        [AllowNull]
        public string Name
        {
            [return: AllowNull]
            get { return name; }
            set { name = value; this.RaisePropertyChange(); }
        }

        string provider;
        [AllowNull]
        public string Provider
        {
            [return: AllowNull]
            get { return provider; }
            set { provider = value; this.RaisePropertyChange(); }
        }

        string signUpLabel;
        [AllowNull]
        public string SignUpLabel
        {
            [return: AllowNull]
            get { return signUpLabel; }
            set { signUpLabel = value; this.RaisePropertyChange(); }
        }
    }
}
