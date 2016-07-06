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
        public string LinkText => (string)GetValue(LinkTextProperty);

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
            SetValue(LinkTextPropertyKey, SelectedItem?.ToString() ?? Header?.ToString());
        }
    }
}
