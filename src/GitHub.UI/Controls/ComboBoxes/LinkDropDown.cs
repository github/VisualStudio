using System.Windows;
using System.Windows.Controls;

namespace GitHub.UI
{
    /// <summary>
    /// A ComboBox that displays as a link with a dropdown.
    /// </summary>
    public class LinkDropDown : ComboBox
    {
        /// <summary>
        /// Defines the <see cref="LinkText"/> property.
        /// </summary>
        static readonly DependencyPropertyKey LinkTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "LinkText",
                typeof(string),
                typeof(LinkDropDown),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Defines the <see cref="Header"/> property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            HeaderedItemsControl.HeaderProperty.AddOwner(
                typeof(LinkDropDown),
                new FrameworkPropertyMetadata(typeof(LinkDropDown), HeaderChanged));

        /// <summary>
        /// Defines the readonly <see cref="LinkText"/> property.
        /// </summary>
        public static readonly DependencyProperty LinkTextProperty =
            LinkTextPropertyKey.DependencyProperty;

        /// <summary>
        /// Initializes static members of the <see cref="LinkDropDown"/> class.
        /// </summary>
        static LinkDropDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LinkDropDown),
                new FrameworkPropertyMetadata(typeof(LinkDropDown)));
        }

        /// <summary>
        /// Gets or sets a header to use as the link text when no item is selected.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Gets the text to display in the link.
        /// </summary>
        public string LinkText
        {
            get { return (string)GetValue(LinkTextProperty); }
            private set { SetValue(LinkTextPropertyKey, value); }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            UpdateLinkText();
        }

        private static void HeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (LinkDropDown)d;
            source.UpdateLinkText();
        }

        private void UpdateLinkText()
        {
            if (SelectedItem != null)
            {
                var item = SelectedItem;

                // HACK: The correct way to do this is to use a ContentPresenter in the control
                // template to display the link text and do a:
                //
                //     ContentTemplateSelector="{TemplateBinding ItemTemplateSelector}"
                //
                // to correctly display the DisplayMemberPath. However I couldn't work out how
                // to do it like this and get the link text looking right. This is a hack that
                // will work as long as DisplayMemberPath is just a property name, which is
                // all we need right now.
                if (string.IsNullOrWhiteSpace(DisplayMemberPath))
                {
                    LinkText = item.ToString();
                }
                else
                {
                    var property = item.GetType().GetProperty(DisplayMemberPath);
                    LinkText = property?.GetValue(item)?.ToString();
                }
            }
            else
            {
                LinkText = Header.ToString();
            }
        }
    }
}
