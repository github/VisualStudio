using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using System.Windows.Shapes;

namespace GitHub.VisualStudio.UI.Views
{
    public partial class WindowController : DialogWindow
    {
        IDisposable disposable;

        public WindowController(IObservable<object> controls)
        {
            InitializeComponent();
            disposable = controls.Subscribe(c =>
            {
                var control = c as UserControl;
                Load(control);
            },
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
