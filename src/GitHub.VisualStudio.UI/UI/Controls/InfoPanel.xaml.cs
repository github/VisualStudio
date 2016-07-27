using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitHub.VisualStudio.UI.Controls
{
    /// <summary>
    /// Interaction logic for InfoPanel.xaml
    /// </summary>
    public partial class InfoPanel : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(InfoPanel), new PropertyMetadata(null, UpdateVisibility));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public InfoPanel()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        static void UpdateVisibility(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (InfoPanel)d;
            control.Visibility = string.IsNullOrEmpty(control.Message) ? Visibility.Collapsed : Visibility.Visible;
        }

        void Dismiss_Click(object sender, RoutedEventArgs e)
        {
            this.Message = string.Empty;
        }
    }
}
