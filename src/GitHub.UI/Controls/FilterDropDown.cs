using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GitHub.UI
{
    public class FilterDropDown : ItemsControl
    {
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                "Filter",
                typeof(string),
                typeof(FilterDropDown),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty HeaderProperty =
            HeaderedContentControl.HeaderProperty.AddOwner(typeof(FilterDropDown));

        public static readonly DependencyProperty MaxDropDownHeightProperty =
            ComboBox.MaxDropDownHeightProperty.AddOwner(typeof(FilterDropDown));

        public static readonly DependencyProperty SelectedItemProperty =
            Selector.SelectedItemProperty.AddOwner(
                typeof(FilterDropDown),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        DropDownButton dropDownButton;
        TextBox filterTextBox;
        Selector selector;
        Button clearButton;

        static FilterDropDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FilterDropDown),
                new FrameworkPropertyMetadata(typeof(FilterDropDown)));
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            dropDownButton = (DropDownButton)Template.FindName("PART_DropDownButton", this);
            filterTextBox = (TextBox)Template.FindName("PART_FilterTextBox", this);
            selector = (Selector)Template.FindName("PART_Selector", this);
            clearButton = (Button)Template.FindName("PART_ClearButton", this);
            dropDownButton.PopupOpened += PopupOpened;
            dropDownButton.PopupClosed += PopupClosed;
            selector.MouseUp += OnSelectorMouseUp;
            clearButton.Click += ClearButtonClick;
            base.OnApplyTemplate();
        }

        private void PopupOpened(object sender, EventArgs e)
        {
            Filter = null;
            selector.SelectedItem = SelectedItem;
            filterTextBox.Focus();
        }

        private void PopupClosed(object sender, EventArgs e)
        {
            if (selector.SelectedItem != null)
            {
                SelectedItem = selector.SelectedItem;
            }
        }

        protected void OnSelectorMouseUp(object sender, MouseButtonEventArgs e)
        {
            dropDownButton.IsOpen = false;
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
            selector.SelectedItem = null;
            dropDownButton.IsOpen = false;
        }
    }
}
