using GitHub.Services;
using GitHub.UI;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Stateless;
using ReactiveUI;
using GitHub.Models;
using GitHub.Authentication;
using System.Diagnostics;

namespace GitHub.VisualStudio.UI
{
    [Export(typeof(IUIController))]
    public class UIController : IUIController, IDisposable
    {

        enum UIState { Start, Auth, TwoFA, Create, Clone, End }
        enum Trigger { Auth = 1, Create = 2, Clone = 3, Next, Previous }

        readonly ExportFactoryProvider factory;

        CompositeDisposable disposables = new CompositeDisposable();
        Subject<object> transition;
        UIControllerFlow currentFlow;
        StateMachine<UIState, Trigger> machine;

        [ImportingConstructor]
        public UIController(IUIProvider uiProvider, IRepositoryHosts hosts)
        {
            factory = uiProvider.GetService<ExportFactoryProvider>();
            
            machine = new StateMachine<UIState, Trigger>(UIState.Start);

            machine.Configure(UIState.Start)
                .Permit(Trigger.Auth, UIState.Auth)
                .PermitIf(Trigger.Create, UIState.Create, () => hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Create, UIState.Auth, () => !hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Clone, UIState.Clone, () => hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Clone, UIState.Auth, () => !hosts.IsLoggedInToAnyHost);

            machine.Configure(UIState.Auth)
                .OnEntry(() =>
                {
                    var twofa = uiProvider.GetService<ITwoFactorDialog>();
                    twofa.WhenAny(x => x.IsShowing, x => x.Value)
                        .Where(x => x)
                        .Subscribe(_ =>
                        {
                            Fire(Trigger.Next);
                        });

                    var d = factory.LoginViewModelFactory.CreateExport();
                    disposables.Add(d);
                    var view = uiProvider.GetService<IViewFor<ILoginDialog>>();
                    view.ViewModel = d.Value;

                    d.Value.AuthenticationResults.Subscribe(result =>
                    {
                        if (result == AuthenticationResult.Success)
                            Fire(Trigger.Next);
                    });
                    transition.OnNext(view);
                })
                .Permit(Trigger.Next, UIState.TwoFA);

            machine.Configure(UIState.TwoFA)
                .SubstateOf(UIState.Auth)
                .OnEntry(() =>
                {
                    var d = uiProvider.GetService<ITwoFactorDialog>();
                    var view = uiProvider.GetService<IViewFor<ITwoFactorDialog>>();
                    view.ViewModel = d;
                    transition.OnNext(view);
                })
                .PermitIf(Trigger.Next, UIState.End, () => currentFlow == UIControllerFlow.Authentication)
                .PermitIf(Trigger.Next, UIState.Create, () => currentFlow == UIControllerFlow.Create)
                .PermitIf(Trigger.Next, UIState.Clone, () => currentFlow == UIControllerFlow.Clone);

            machine.Configure(UIState.Create)
                .OnEntry(() =>
                {
                    var d = uiProvider.GetService<ICreateRepoViewModel>();
                    var view = uiProvider.GetService<IViewFor<ICreateRepoViewModel>>();
                    view.ViewModel = d;
                    transition.OnNext(view);
                })
                .Permit(Trigger.Next, UIState.End);

            machine.Configure(UIState.Clone)
                .OnEntry(() =>
                {
                    var d = uiProvider.GetService<ICloneRepoDialog>();
                    
                    var view = uiProvider.GetService<IViewFor<ICloneRepoDialog>>();
                    view.ViewModel = d;
                    transition.OnNext(view);
                })
                .Permit(Trigger.Next, UIState.End);

            machine.Configure(UIState.End)
                .OnEntry(() =>
                {
                    transition.OnCompleted();
                    transition.Dispose();
                })
                .Permit(Trigger.Next, UIState.Start);
        }

        void Fire(Trigger next)
        {
            Debug.WriteLine("Firing {0}", next);
            machine.Fire(next);
        }

        public IObservable<object> SelectFlow(UIControllerFlow choice)
        {
            currentFlow = choice;
            transition = new Subject<object>();
            transition.Subscribe((o) => { }, _ => Fire(Trigger.Next));
            return transition;
        }

        public void Start()
        {
            Fire((Trigger)(int)currentFlow);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposables.Dispose();
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
        #endregion

    }
}
