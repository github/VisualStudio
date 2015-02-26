using System;
using System.Windows;
using System.Windows.Controls;

namespace GitHub.UI
{
    public class AppendingPathTextBox : TextBox
    {
        public static readonly DependencyProperty ParentFolderPathProperty =
            DependencyProperty.Register("ParentFolderPath", typeof(string), typeof(AppendingPathTextBox), new FrameworkPropertyMetadata(null, OnUpdateParentFolder));

        public string ParentFolderPath
        {
            get { return (string)GetValue(ParentFolderPathProperty); }
            set { SetValue(ParentFolderPathProperty, value); }
        }

        public string ChildFolderName
        {
            get { return (string)GetValue(ChildFolderNameProperty); }
            set { SetValue(ChildFolderNameProperty, value); }
        }

        public static readonly DependencyProperty ChildFolderNameProperty =
            DependencyProperty.Register("ChildFolderName", typeof(string), typeof(AppendingPathTextBox), new PropertyMetadata(null, UpdateVisibilities));

        public Visibility PathSeparatorVisibility
        {
            get { return (Visibility)GetValue(PathSeparatorVisibilityProperty); }
            set { SetValue(PathSeparatorVisibilityProperty, value); }
        }

        public static readonly DependencyProperty PathSeparatorVisibilityProperty =
         DependencyProperty.Register("PathSeparatorVisibility", typeof(Visibility), typeof(AppendingPathTextBox), new PropertyMetadata(Visibility.Visible));

        public Visibility ChildFolderVisibility
        {
            get { return (Visibility)GetValue(ChildFolderVisibilityProperty); }
            set { SetValue(ChildFolderVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ChildFolderVisibilityProperty =
         DependencyProperty.Register("ChildFolderVisibility", typeof(Visibility), typeof(AppendingPathTextBox), new PropertyMetadata(Visibility.Visible));

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            ParentFolderPath = Text;
            base.OnTextChanged(e);
        }

        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (!e.Handled)
            {
                this.SelectAll();
            }
        }

        static void OnUpdateParentFolder(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AppendingPathTextBox)d;
            control.Text = control.ParentFolderPath;
            UpdatePathSeparatorVisibility(control);
            UpdateChildFolderVisibilityVisibility(control);
        }

        static void UpdateVisibilities(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AppendingPathTextBox)d;
            UpdatePathSeparatorVisibility(control);
            UpdateChildFolderVisibilityVisibility(control);
        }

        static void UpdateChildFolderVisibilityVisibility(AppendingPathTextBox control)
        {
            control.ChildFolderVisibility = string.IsNullOrEmpty(control.ParentFolderPath) ? Visibility.Collapsed : Visibility.Visible;
        }

        static void UpdatePathSeparatorVisibility(AppendingPathTextBox control)
        {
            if (!string.IsNullOrEmpty(control.ParentFolderPath))
            {
                control.PathSeparatorVisibility = control.ParentFolderPath.EndsWith("\\", StringComparison.Ordinal)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            else
            {
                control.PathSeparatorVisibility = Visibility.Collapsed;
            }
        }
    }
}