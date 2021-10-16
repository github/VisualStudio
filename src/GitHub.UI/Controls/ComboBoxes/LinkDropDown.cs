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
        /// Defines the <see cref="LinkItem"/> property.
        /// </summary>
        static readonly DependencyPropertyKey LinkItemPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "LinkItem",
                typeof(object),
                typeof(LinkDropDown),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Defines the <see cref="LinkItemTemplate"/> property.
        /// </summary>
        public static readonly DependencyProperty LinkItemTemplateProperty =
            DependencyProperty.Register(
                "LinkItemTemplate",
                typeof(DataTemplate),
                typeof(LinkDropDown));

        /// <summary>
        /// Defines the <see cref="Header"/> property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            HeaderedItemsControl.HeaderProperty.AddOwner(
                typeof(LinkDropDown),
                new FrameworkPropertyMetadata(typeof(LinkDropDown), HeaderChanged));

        /// <summary>
        /// Defines the readonly <see cref="LinkItem"/> property.
        /// </summary>
        public static readonly DependencyProperty LinkItemProperty =
            LinkItemPropertyKey.DependencyProperty;

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
        /// Gets or sets a header to use in the link when no item is selected.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Gets the data to display in the link.
        /// </summary>
        public object LinkItem
        {
            get { return (string)GetValue(LinkItemProperty); }
            private set { SetValue(LinkItemPropertyKey, value); }
        }

        /// <summary>
        /// Gets the template to use to display the link.
        /// </summary>
        public object LinkItemTemplate
        {
            get { return (string)GetValue(LinkItemTemplateProperty); }
            set { SetValue(LinkItemTemplateProperty, value); }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
           UpdateLinkItem();
        }

        private static void HeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (LinkDropDown)d;
            source.UpdateLinkItem();
        }

        private void UpdateLinkItem()
        {
            LinkItem = SelectedItem ?? Header;
        }
    }
}
