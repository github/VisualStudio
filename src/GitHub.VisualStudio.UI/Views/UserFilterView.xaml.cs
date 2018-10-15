using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.Views
{
    public partial class UserFilterView : UserControl
    {
        public UserFilterView()
        {
            InitializeComponent();
        }

        public void FocusSearchBox() => searchBox.Focus();

        private void ListBoxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var fe = e.Source as FrameworkElement;

            if (fe?.DataContext is IActorViewModel vm)
            {
                ((IUserFilterViewModel)DataContext).Selected = vm;
            }
        }
    }
}
