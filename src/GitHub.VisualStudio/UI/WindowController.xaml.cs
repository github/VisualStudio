using System;
using System.ComponentModel;
using System.Windows.Controls;
using GitHub.UI;
using GitHub.ViewModels;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.VisualStudio.UI
{
    public partial class WindowController : DialogWindow, IDisposable
    {
        IDisposable subscription;
        IObservable<IView> controls;
        Func<IView, bool> shouldLoad;
        Func<IView, bool> shouldStop;

        public WindowController(IObservable<IView> controls,
            Func<IView, bool> shouldLoad = null,
            Func<IView, bool> shouldStop = null)
        {
            this.controls = controls;
            this.shouldLoad = shouldLoad;
            this.shouldStop = shouldStop;

            InitializeComponent();
            Initialize();
        }

        void Initialize()
        {
            subscription = controls.Subscribe(c =>
            {
                if (shouldLoad == null || shouldLoad(c))
                    Load(c);
                if (shouldStop != null && shouldStop(c))
                {
                    Stop();
                    Close();
                }
            });
            Closed += (s, e) => Dispose();
        }

        public void Load(IView view)
        {
            var viewModel = view.ViewModel as IViewModel;
            if (viewModel != null)
                Title = viewModel.Title;

            var control = view as UserControl;
            if (control != null)
            {
                Container.Children.Clear();
                Container.Children.Add(control);
            }
        }

        public void Stop()
        {
            subscription?.Dispose();
            subscription = null;
        }

        bool disposed = false;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    Stop();
                    disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
