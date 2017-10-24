using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using ReactiveUI;
using GitHub.App.Factories;
using GitHub.Exports;
using GitHub.Controllers;
using GitHub.Logging;
using Serilog;

namespace GitHub.VisualStudio.UI
{
    [Export(typeof(IUIProvider))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class UIProviderDispatcher : IUIProvider
    {
        readonly IUIProvider theRealProvider;

        [ImportingConstructor]
        public UIProviderDispatcher([Import(typeof(Microsoft.VisualStudio.Shell.SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            theRealProvider = serviceProvider.GetServiceSafe<IUIProvider>();
        }

        public IUIController Configure(UIControllerFlow flow, IConnection connection = null, ViewWithData data = null) => theRealProvider.Configure(flow, connection, data);

        public IView GetView(UIViewType which, ViewWithData data = null) => theRealProvider.GetView(which, data);

        public void Run(IUIController controller) => theRealProvider.Run(controller);

        public IUIController Run(UIControllerFlow flow) => theRealProvider.Run(flow);

        public void RunInDialog(IUIController controller) => theRealProvider.RunInDialog(controller);

        public void RunInDialog(UIControllerFlow flow, IConnection connection = null) => theRealProvider.RunInDialog(flow, connection);

        public void StopUI(IUIController controller) => theRealProvider.StopUI(controller);
    }

    public class UIProvider : IUIProvider, IDisposable
    {
        static readonly ILogger log = LogManager.ForContext<UIProvider>();

        WindowController windowController;

        readonly CompositeDisposable disposables = new CompositeDisposable();

        readonly IGitHubServiceProvider serviceProvider;

        public UIProvider(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IView GetView(UIViewType which, ViewWithData data = null)
        {
            var uiFactory = serviceProvider.GetService<IUIFactory>();
            var pair = uiFactory.CreateViewAndViewModel(which);
            pair.ViewModel.Initialize(data);
            pair.View.DataContext = pair.ViewModel;
            return pair.View;
        }

        public IUIController Configure(UIControllerFlow flow, IConnection connection = null, ViewWithData data = null)
        {
            var controller = new UIController(serviceProvider);
            disposables.Add(controller);
            var listener = controller.Configure(flow, connection, data).Publish().RefCount();

            listener.Subscribe(_ => { }, () =>
            {
                StopUI(controller);
            });

            // if the flow is authentication, we need to show the login dialog. and we can't
            // block the main thread on the subscriber, it'll block other handlers, so we're doing
            // this on a separate thread and posting the dialog to the main thread
            listener
                .Where(c => c.Flow == UIControllerFlow.Authentication)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(c =>
                {
                    // nothing to do, we already have a dialog
                    if (windowController != null)
                        return;
                    RunModalDialogForAuthentication(c.Flow, listener, c).Forget();
                });

            return controller;
        }

        public IUIController Run(UIControllerFlow flow)
        {
            var controller = Configure(flow);
            controller.Start();
            return controller;
        }

        public void Run(IUIController controller)
        {
            controller.Start();
        }

        public void RunInDialog(UIControllerFlow flow, IConnection connection = null)
        {
            var controller = Configure(flow, connection);
            RunInDialog(controller);
        }

        public void RunInDialog(IUIController controller)
        {
            var listener = controller.TransitionSignal;

            windowController = new UI.WindowController(listener);
            windowController.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            EventHandler stopUIAction = (s, e) =>
            {
                StopUI(controller);
            };
            windowController.Closed += stopUIAction;
            listener.Subscribe(_ => { }, () =>
            {
                windowController.Closed -= stopUIAction;
                windowController.Close();
                StopUI(controller);
            });

            controller.Start();
            windowController.ShowModal();
            windowController = null;
        }

        public void StopUI(IUIController controller)
        {
            try
            {
                if (!controller.IsStopped)
                    controller.Stop();
                disposables.Remove(controller);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to dispose UI.");
            }
        }

        async Task RunModalDialogForAuthentication(UIControllerFlow flow, IObservable<LoadData> listener, LoadData initiaLoadData)
        {
            await ThreadingHelper.SwitchToMainThreadAsync();
            windowController = new WindowController(listener,
                (v, f) => f == flow,
                (v, f) => f != flow);
            windowController.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            windowController.Load(initiaLoadData.View);
            windowController.ShowModal();
            windowController = null;
        }

        bool disposed;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                if (disposables != null)
                    disposables.Dispose();
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
