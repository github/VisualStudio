using GitHub.UI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GitHub.ViewModels;
using System.ComponentModel;
using GitHub.Services;
using GitHub.Extensions;
using System.Windows.Input;
using GitHub.Primitives;
using GitHub.VisualStudio.Helpers;
using Colors = System.Windows.Media.Colors;

namespace GitHub.VisualStudio.UI.Controls
{
    public partial class InfoPanel : UserControl, IInfoPanel, INotifyPropertyChanged, INotifyPropertySource
    {
        static SolidColorBrush WarningColorBrush = new SolidColorBrush(Colors.DarkRed);
        static SolidColorBrush InfoColorBrush = new SolidColorBrush(Colors.Black);

        static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(InfoPanel), new PropertyMetadata(String.Empty, UpdateMessage));

        static readonly DependencyProperty MessageTypeProperty =
            DependencyProperty.Register(nameof(MessageType), typeof(MessageType), typeof(InfoPanel), new PropertyMetadata(MessageType.Information, UpdateIcon));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public MessageType MessageType
        {
            get { return (MessageType)GetValue(MessageTypeProperty); }
            set { SetValue(MessageTypeProperty, value); }
        }

        Octicon icon;
        public Octicon Icon
        {
            get { return icon; }
            private set { icon = value; RaisePropertyChanged(nameof(Icon)); }
        }

        Brush iconColor;
        public Brush IconColor
        {
            get { return iconColor; }
            private set { iconColor = value; RaisePropertyChanged(nameof(IconColor)); }
        }

        ICommand linkCommand;
        public ICommand LinkCommand
        {
            get { return linkCommand; }
            set { linkCommand = value; RaisePropertyChanged(nameof(LinkCommand)); }
        }

        static InfoPanel()
        {
            WarningColorBrush.Freeze();
            InfoColorBrush.Freeze();
        }

        static IVisualStudioBrowser browser;
        static IVisualStudioBrowser Browser
        {
            get
            {
                if (browser == null)
                    browser = Services.GitHubServiceProvider.TryGetService<IVisualStudioBrowser>();
                return browser;
            }
        }

        public InfoPanel()
        {
            InitializeComponent();

            DataContext = this;
            Icon = Octicon.info;
            IconColor = InfoColorBrush;

            LinkCommand = new RelayCommand(x =>
            {
                if (!String.IsNullOrEmpty(x as string))
                    Browser.OpenUrl(new Uri((string)x));
            });
        }

        static void UpdateMessage(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (InfoPanel)d;
            var msg = e.NewValue as string;
            control.Visibility = String.IsNullOrEmpty(msg) ? Visibility.Collapsed : Visibility.Visible;
        }

        static void UpdateIcon(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (InfoPanel)d;
            control.Icon = (MessageType)e.NewValue == MessageType.Warning ? Octicon.alert : Octicon.info;
            control.IconColor = control.Icon == Octicon.alert ? WarningColorBrush : InfoColorBrush;
        }

        void Dismiss_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(MessageProperty, String.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
