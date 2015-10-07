using System;
using System.ComponentModel;
using System.Windows.Controls;
using GitHub.UI;
using GitHub.ViewModels;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.VisualStudio.UI
{
    public partial class WindowController : DialogWindow
    {
        public WindowController(IObservable<IView> controls)
        {
            InitializeComponent();
            controls.Subscribe(c => Load(c));
        }

        public void Load(IView view)
        {
            var viewModel = view.ViewModel as IViewModel;
            if (viewModel != null)
            {
                Title = viewModel.Title;
            }
            var control = view as UserControl;
            if (control != null)
            {
                Container.Children.Clear();
                Container.Children.Add(control);
            }
        }
    }
}
