using System;
using System.Windows.Controls;

namespace GitHub.VisualStudio.Views
{
    /// <summary>
    /// Interaction logic for UserFilterView.xaml
    /// </summary>
    public partial class UserFilterView : UserControl
    {
        public UserFilterView()
        {
            InitializeComponent();
        }

        public void FocusSearchBox() => searchBox.Focus();
    }
}
