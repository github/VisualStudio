using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Controls;
using GitHub.Authentication;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using Stateless;

namespace GitHub.Controllers
{
    [Export(typeof(IUIController))]
    public class UIController : IUIController, IDisposable
    {
        enum Trigger { Auth = 1, Create = 2, Clone = 3, Next, Previous, Finish }

        readonly ExportFactoryProvider factory;
        readonly IUIProvider uiProvider;

        readonly CompositeDisposable disposables = new CompositeDisposable();
        readonly StateMachine<UIViewType, Trigger> machine;
        Subject<UserControl> transition;
        UIControllerFlow currentFlow;

        [ImportingConstructor]
        public UIController(IUIProvider uiProvider, IRepositoryHosts hosts, ExportFactoryProvider factory)
        {
            this.factory = factory;
            this.uiProvider = uiProvider;

            machine = new StateMachine<UIViewType, Trigger>(UIViewType.None);

            machine.Configure(UIViewType.None)
                .Permit(Trigger.Auth, UIViewType.Login)
                .PermitIf(Trigger.Create, UIViewType.Create, () => hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Create, UIViewType.Login, () => !hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Clone, UIViewType.Clone, () => hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Clone, UIViewType.Login, () => !hosts.IsLoggedInToAnyHost);

            machine.Configure(UIViewType.Login)
                .OnEntry(() =>
                {
                    var dvm = factory.GetViewModel(UIViewType.Login);
                    disposables.Add(dvm);
                    var viewModel = dvm.Value as ILoginViewModel;
                    Debug.Assert(viewModel != null, "The view model must implement ILoginViewModel");
                    viewModel.AuthenticationResults.Subscribe(result =>
                    {
                        if (result == AuthenticationResult.Success)
                            Fire(Trigger.Finish); // Takes us to clone or create.
                    });

                    var dv = factory.GetView(UIViewType.Login);
                    disposables.Add(dv);
                    var view = dv.Value;
                    view.ViewModel = viewModel;

                    var twofa = uiProvider.GetService<ITwoFactorViewModel>();
                    twofa.WhenAny(x => x.IsShowing, x => x.Value)
                        .Where(x => x)
                        .Subscribe(_ =>
                        {
                            Fire(Trigger.Next);
                        });
                    LoadView(view);
                })
                .Permit(Trigger.Next, UIViewType.TwoFactor)
                .PermitIf(Trigger.Finish, UIViewType.Create, () => currentFlow == UIControllerFlow.Create)
                .PermitIf(Trigger.Finish, UIViewType.Clone, () => currentFlow == UIControllerFlow.Clone);

            machine.Configure(UIViewType.TwoFactor)
                .SubstateOf(UIViewType.Login)
                .OnEntry(() =>
                {
                    var view = SetupView(UIViewType.TwoFactor);
                    LoadView(view);
                })
                .PermitIf(Trigger.Next, UIViewType.End, () => currentFlow == UIControllerFlow.Authentication)
                .PermitIf(Trigger.Next, UIViewType.Create, () => currentFlow == UIControllerFlow.Create)
                .PermitIf(Trigger.Next, UIViewType.Clone, () => currentFlow == UIControllerFlow.Clone);

            machine.Configure(UIViewType.Create)
                .OnEntry(() =>
                {
                    var view = SetupView(UIViewType.Create);
                    LoadView(view);
                })
                .Permit(Trigger.Next, UIViewType.End);

            machine.Configure(UIViewType.Clone)
                .OnEntry(() =>
                {
                    var view = SetupView(UIViewType.Clone);
                    LoadView(view);
                })
                .Permit(Trigger.Next, UIViewType.End);

            machine.Configure(UIViewType.End)
                .OnEntry(() =>
                {
                    transition.OnCompleted();
                    transition.Dispose();
                })
                .Permit(Trigger.Next, UIViewType.None);
        }

        private void LoadView(IView view)
        {
            transition.OnNext(view as UserControl);
        }

        IView SetupView(UIViewType viewType)
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

        public IObservable<UserControl> SelectFlow(UIControllerFlow choice)
        {
            currentFlow = choice;
            transition = new Subject<UserControl>();
            transition.Subscribe((o) => { }, _ => Fire(Trigger.Next));
            return transition;
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
