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
    /// Interaction logic for ErrorStrip.xaml
    /// </summary>
    public partial class ErrorStrip : UserControl
    {
        public ErrorStrip()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MessageProperty =
       DependencyProperty.Register(nameof(Message), typeof(string), typeof(ErrorStrip), new PropertyMetadata(null, UpdateVisibilities));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        static void UpdateVisibilities(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ErrorStrip)d;          
            control.Visibility = string.IsNullOrEmpty(control.Message) ? Visibility.Collapsed : Visibility.Visible;
        }

    }
}
