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
        readonly IObservable<LoadData> controls;
        readonly Func<IView, UIControllerFlow, bool> shouldLoad;
        readonly Func<IView, UIControllerFlow, bool> shouldStop;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controls">Observable that provides controls to host in this window</param>
        /// <param name="shouldLoad">If set, this condition will be checked before loading each control</param>
        /// <param name="shouldStop">If set, this condition will be checked to determine when to close this window</param>
        public WindowController(IObservable<LoadData> controls,
            Func<IView, UIControllerFlow, bool> shouldLoad = null,
            Func<IView, UIControllerFlow, bool> shouldStop = null)
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
                if (shouldLoad == null || shouldLoad(c.View, c.Flow))
                    Load(c.View);
                if (shouldStop != null && shouldStop(c.View, c.Flow))
                {
                    Stop();
                    Close();
                }
            });
            Closed += (s, e) => Dispose();
        }

        public void Load(IView view)
        {
            var viewModel = view.ViewModel as IDialogViewModel;
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
        }

        bool disposed = false;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                    Stop();
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
