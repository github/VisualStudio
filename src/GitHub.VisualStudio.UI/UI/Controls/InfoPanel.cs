using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.Services;
using GitHub.UI;

namespace GitHub.VisualStudio.UI.Controls
{
    /// <summary>
    /// Displays informational or error message markdown in a banner.
    /// </summary>
    public class InfoPanel : Control
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(InfoPanel));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(Octicon),
                typeof(InfoPanel),
                new FrameworkPropertyMetadata(Octicon.info));

        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register(
                nameof(ShowCloseButton),
                typeof(bool),
                typeof(InfoPanel));

        static IVisualStudioBrowser browser;
        Button closeButton;

        static InfoPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(InfoPanel),
                new FrameworkPropertyMetadata(typeof(InfoPanel)));
            DockPanel.DockProperty.OverrideMetadata(
                typeof(InfoPanel),
                new FrameworkPropertyMetadata(Dock.Top));
        }

        public InfoPanel()
        {
            var commandBinding = new CommandBinding(Markdig.Wpf.Commands.Hyperlink);
            commandBinding.Executed += OpenHyperlink;
            CommandBindings.Add(commandBinding);
        }

        /// <summary>
        /// Gets or sets the message in markdown.
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        public Octicon Icon
        {
            get => (Octicon)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        static IVisualStudioBrowser Browser
        {
            get
            {
                if (browser == null)
                    browser = Services.GitHubServiceProvider.TryGetService<IVisualStudioBrowser>();
                return browser;
            }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            closeButton = (Button)Template.FindName("PART_CloseButton", this);
            closeButton.Click += CloseButtonClicked;
        }

        void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Message = null;
        }

        void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            var url = e.Parameter.ToString();

            if (!string.IsNullOrEmpty(url))
                Browser.OpenUrl(new Uri(url));
        }
    }
}
