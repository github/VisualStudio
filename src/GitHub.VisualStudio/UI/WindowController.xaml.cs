using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.VisualStudio.UI
{
    public partial class WindowController : DialogWindow
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
