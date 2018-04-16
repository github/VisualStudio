using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GitHub.Models;

namespace GitHub.VisualStudio.UI.Controls
{
    public partial class AccountAvatar : UserControl, ICommandSource
    {
        public static readonly DependencyProperty AccountProperty =
            DependencyProperty.Register(
                nameof(Account),
                typeof(IAccount),
                typeof(AccountAvatar));
        public static readonly DependencyProperty CommandProperty =
            ButtonBase.CommandProperty.AddOwner(typeof(AccountAvatar));
        public static readonly DependencyProperty CommandParameterProperty =
            ButtonBase.CommandParameterProperty.AddOwner(typeof(AccountAvatar));
        public static readonly DependencyProperty CommandTargetProperty =
            ButtonBase.CommandTargetProperty.AddOwner(typeof(AccountAvatar));

        public AccountAvatar()
        {
            InitializeComponent();
        }

        public IAccount Account
        {
            get { return (IAccount)GetValue(AccountProperty); }
            set { SetValue(AccountProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }
    }
}
