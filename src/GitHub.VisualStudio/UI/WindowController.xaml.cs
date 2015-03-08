using System;
using System.Windows.Controls;
using GitHub.UI;
using GitHub.ViewModels;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.VisualStudio.UI
{
    public partial class WindowController : DialogWindow
    {
        IDisposable disposable;

        public WindowController(IObservable<UserControl> controls)
        {
            InitializeComponent();
            disposable = controls.Subscribe(c => Load(c), Close);
        }

        protected override void OnClosed(EventArgs e)
        {
            disposable.Dispose();
            base.OnClosed(e);
        }

        public void Load(UserControl control)
        {
            var view = control as IView;
            if (view != null)
            {
                var viewModel = view.ViewModel as IViewModel;
                if (viewModel != null)
                {
                    Title = viewModel.Title;
                }
            }
            Container.Children.Clear();
            Container.Children.Add(control);
        }
    }
}
