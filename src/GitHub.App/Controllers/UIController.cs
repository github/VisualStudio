using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using Stateless;
using NullGuard;

namespace GitHub.Controllers
{
    [Export(typeof(IUIController))]
    public class UIController : IUIController, IDisposable
    {
        enum Trigger { Auth = 1, Create = 2, Clone = 3, Publish = 4, Next, Previous, Finish }

        readonly IExportFactoryProvider factory;
        readonly IUIProvider uiProvider;
        readonly IRepositoryHosts hosts;

        readonly CompositeDisposable disposables = new CompositeDisposable();
        readonly StateMachine<UIViewType, Trigger> machine;
        Subject<UserControl> transition;
        UIControllerFlow currentFlow;

        [ImportingConstructor]
        public UIController(IUIProvider uiProvider, IRepositoryHosts hosts, IExportFactoryProvider factory)
        {
            this.factory = factory;
            this.uiProvider = uiProvider;
            this.hosts = hosts;

#if DEBUG
            if (Application.Current != null && !Splat.ModeDetector.InUnitTestRunner())
            {
                var waitDispatcher = RxApp.MainThreadScheduler as WaitForDispatcherScheduler;
                if (waitDispatcher != null)
                {
                    Debug.Assert(DispatcherScheduler.Current.Dispatcher == Application.Current.Dispatcher,
                       "DispatcherScheduler is set correctly");
                }
                else
                {
                    Debug.Assert(((DispatcherScheduler)RxApp.MainThreadScheduler).Dispatcher == Application.Current.Dispatcher,
                        "The MainThreadScheduler is using the wrong dispatcher");
                }
            }
#endif
            machine = new StateMachine<UIViewType, Trigger>(UIViewType.None);

            machine.Configure(UIViewType.Login)
                .OnEntry(() =>
                {
                    RunView(UIViewType.Login);
                })
                .Permit(Trigger.Next, UIViewType.TwoFactor)
                // Added the following line to make it easy to login to both GitHub and GitHub Enterprise 
                // in DesignTimeStyleHelper in order to test Publish.
                .PermitIf(Trigger.Finish, UIViewType.End, () => currentFlow == UIControllerFlow.Authentication)
                .PermitIf(Trigger.Finish, UIViewType.Create, () => currentFlow == UIControllerFlow.Create)
                .PermitIf(Trigger.Finish, UIViewType.Clone, () => currentFlow == UIControllerFlow.Clone)
                .PermitIf(Trigger.Finish, UIViewType.Publish, () => currentFlow == UIControllerFlow.Publish);

            machine.Configure(UIViewType.TwoFactor)
                .SubstateOf(UIViewType.Login)
                .OnEntry(() =>
                {
                    RunView(UIViewType.TwoFactor);
                })
                .PermitIf(Trigger.Next, UIViewType.End, () => currentFlow == UIControllerFlow.Authentication)
                .PermitIf(Trigger.Next, UIViewType.Create, () => currentFlow == UIControllerFlow.Create)
                .PermitIf(Trigger.Next, UIViewType.Clone, () => currentFlow == UIControllerFlow.Clone)
                .PermitIf(Trigger.Next, UIViewType.Publish, () => currentFlow == UIControllerFlow.Publish);

            machine.Configure(UIViewType.Create)
                .OnEntry(() =>
                {
                    RunView(UIViewType.Create);
                })
                .Permit(Trigger.Next, UIViewType.End);

            machine.Configure(UIViewType.Clone)
                .OnEntry(() =>
                {
                    RunView(UIViewType.Clone);
                })
                .Permit(Trigger.Next, UIViewType.End);

            machine.Configure(UIViewType.Publish)
                .OnEntry(() =>
                {
                    RunView(UIViewType.Publish);
                })
                .Permit(Trigger.Next, UIViewType.End);

            machine.Configure(UIViewType.End)
                .OnEntry(() =>
                {
                    transition.OnCompleted();
                    transition.Dispose();
                    transition = null;
                })
                .Permit(Trigger.Next, UIViewType.None);
        }

        public IObservable<UserControl> SelectFlow(UIControllerFlow choice, [AllowNull] IConnection connection)
        {
            IRepositoryHost host = RepositoryHosts.DisconnectedRepositoryHost;
            if (connection != null)
                host = hosts.LookupHost(connection.HostAddress);

            machine.Configure(UIViewType.None)
                .Permit(Trigger.Auth, UIViewType.Login)
                .PermitIf(Trigger.Create, UIViewType.Create, () => host.IsLoggedIn)
                .PermitIf(Trigger.Create, UIViewType.Login, () => !host.IsLoggedIn)
                .PermitIf(Trigger.Clone, UIViewType.Clone, () => host.IsLoggedIn)
                .PermitIf(Trigger.Clone, UIViewType.Login, () => !host.IsLoggedIn)
                .PermitIf(Trigger.Publish, UIViewType.Publish, () => host.IsLoggedIn)
                .PermitIf(Trigger.Publish, UIViewType.Login, () => !host.IsLoggedIn);

            currentFlow = choice;
            transition = new Subject<UserControl>();
            transition.Subscribe(_ => { }, _ => Fire(Trigger.Next));
            return transition;
        }

        void RunView(UIViewType viewType)
        {
            var view = CreateViewAndViewModel(viewType);
            transition.OnNext(view as UserControl);
            SetupView(viewType, view);
        }

        void SetupView(UIViewType viewType, IView view)
        {
            if (viewType == UIViewType.Login)
            {
                // we're setting up the login dialog, we need to setup the 2fa as
                // well to continue the flow if it's needed, since the
                // authenticationresult callback won't happen until
                // everything is done
                var twofa = uiProvider.GetService<ITwoFactorViewModel>();
                twofa.WhenAny(x => x.IsShowing, x => x.Value)
                    .Where(x => x)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Next));

                view.Done
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Finish));
            }
            else
                view.Done
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Next));
        }

        IView CreateViewAndViewModel(UIViewType viewType)
        {
            IViewModel viewModel;
            if (viewType == UIViewType.TwoFactor)
            {
                viewModel = uiProvider.GetService<ITwoFactorViewModel>();
            }
            else
            {
                var dvm = factory.GetViewModel(viewType);
                disposables.Add(dvm);
                viewModel = dvm.Value;
            }
            
            var dv = factory.GetView(viewType);
            disposables.Add(dv);
            var view = dv.Value;

            view.ViewModel = viewModel;

            return view;
        }

        void Fire(Trigger next)
        {
            Debug.WriteLine("Firing {0}", next);
            machine.Fire(next);
        }

        public void Start()
        {
            Fire((Trigger)(int)currentFlow);
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposables.Dispose();
                    if (transition != null)
                        transition.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
