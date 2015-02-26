using System;
using System.Windows;
using System.Windows.Controls;

namespace DesignTimeStyleHelper
{
    /// <summary>
    /// Interaction logic for WindowController.xaml
    /// </summary>
    public partial class WindowController : Window
    {
        IDisposable disposable;

        public WindowController(IObservable<UserControl> controls)
        {
            InitializeComponent();

            disposable = controls.Subscribe(c => Load(c),
            Close
            );
        }

        protected override void OnClosed(EventArgs e)
        {
            disposable.Dispose();
            base.OnClosed(e);
        }

        public void Load(UserControl control)
        {
            Container.Children.Clear();
            Container.Children.Add(control);
        }
    }
}
