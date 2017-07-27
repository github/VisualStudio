using GitHub.Exports;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using GitHub.Extensions;

namespace GitHub.App.Factories
{
    [Export(typeof(IUIFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UIFactory : IUIFactory
    {
        readonly IExportFactoryProvider factory;

        [ImportingConstructor]
        public UIFactory(IExportFactoryProvider factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Creates View/ViewModel instances for the specified <paramref name="viewType"/> if they
        /// haven't been created yet in the current flow
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns>true if the View/ViewModel didn't exist and had to be created</returns>
        public IUIPair CreateViewAndViewModel(UIViewType viewType)
        {
            return new UIPair(viewType, factory.GetView(viewType), factory.GetViewModel(viewType));
        }

        protected virtual void Dispose(bool disposing)
        {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    /// <summary>
    /// This class holds ExportLifetimeContexts (i.e., Lazy Disposable containers) for IView and IViewModel objects
    /// A view type (login, clone, etc) is composed of a pair of view and viewmodel, which this class represents.
    /// </summary>
    public class UIPair : IUIPair
    {
        readonly ExportLifetimeContext<IView> view;
        readonly ExportLifetimeContext<IViewModel> viewModel;
        readonly CompositeDisposable handlers = new CompositeDisposable();
        readonly UIViewType viewType;

        public UIViewType ViewType => viewType;
        public IView View => view.Value;
        public IViewModel ViewModel => viewModel?.Value;

        /// <param name="type">The UIViewType</param>
        /// <param name="v">The IView</param>
        /// <param name="vm">The IViewModel. Might be null because the 2fa view shares the same viewmodel as the login dialog, so it's
        /// set manually in the view outside of this</param>
        public UIPair(UIViewType type, ExportLifetimeContext<IView> v, ExportLifetimeContext<IViewModel> vm)
        {
            Guard.ArgumentNotNull(v, nameof(v));

            viewType = type;
            view = v;
            viewModel = vm;
            handlers = new CompositeDisposable();
        }

        /// <summary>
        /// Register disposable event handlers or observable subscriptions so they get cleared
        /// when the View/Viewmodel get disposed/destroyed
        /// </summary>
        /// <param name="disposable"></param>
        public void AddHandler(IDisposable disposable)
        {
            handlers.Add(disposable);
        }

        public void ClearHandlers()
        {
            handlers.Dispose();
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;
                if (!handlers.IsDisposed)
                    handlers.Dispose();
                view?.Dispose();
                viewModel?.Dispose();
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
