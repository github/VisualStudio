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
using GitHub.Exports;

namespace GitHub.VisualStudio.UI
{
    [Export(typeof(IUIController))]
    public class UIController : IUIController, IDisposable
    {
        enum Trigger { Auth = 1, Create = 2, Clone = 3, Next, Previous }

        readonly ExportFactoryProvider factory;

        CompositeDisposable disposables = new CompositeDisposable();
        Subject<object> transition;
        UIControllerFlow currentFlow;
        StateMachine<UIViewType, Trigger> machine;

        [ImportingConstructor]
        public UIController(IUIProvider uiProvider, IRepositoryHosts hosts, ExportFactoryProvider factory)
        {
            this.factory = factory;

            machine = new StateMachine<UIViewType, Trigger>(UIViewType.Start);

            machine.Configure(UIViewType.None)
                .Permit(Trigger.Auth, UIViewType.Login)
                .PermitIf(Trigger.Create, UIViewType.Create, () => hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Create, UIViewType.Login, () => !hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Clone, UIViewType.Clone, () => hosts.IsLoggedInToAnyHost)
                .PermitIf(Trigger.Clone, UIViewType.Login, () => !hosts.IsLoggedInToAnyHost);

            machine.Configure(UIViewType.Login)
                .OnEntry(() =>
                {
                    var disposable = factory.GetViewModel(UIViewType.Login);
                    disposables.Add(disposable);
                    var viewModel = disposable.Value as ILoginViewModel;

                    viewModel.AuthenticationResults.Subscribe(result =>
                    {
                        if (result == AuthenticationResult.Success)
                            Fire(Trigger.Next);
                    });

                    disposable = factory.GetView(UIViewType.Login);
                    disposables.Add(disposable);
                    var view = disposable.Value;
                    view.ViewModel = viewModel;

                    var twofa = factory.GetViewModel(UIViewType.TwoFactor).Value;
                    twofa.WhenAny(x => x.IsShowing, x => x.Value)
                        .Where(x => x)
                        .Subscribe(_ =>
                        {
                            Fire(Trigger.Next);
                        });

                    transition.OnNext(view);
                })
                .Permit(Trigger.Next, UIViewType.TwoFactor);

            machine.Configure(UIViewType.TwoFactor)
                .SubstateOf(UIViewType.Login)
                .OnEntry(() =>
                {
                    var view = SetupView(UIViewType.TwoFactor);
                    transition.OnNext(view);
                })
                .PermitIf(Trigger.Next, UIViewType.End, () => currentFlow == UIControllerFlow.Authentication)
                .PermitIf(Trigger.Next, UIViewType.Create, () => currentFlow == UIControllerFlow.Create)
                .PermitIf(Trigger.Next, UIViewType.Clone, () => currentFlow == UIControllerFlow.Clone);

            machine.Configure(UIViewType.Create)
                .OnEntry(() =>
                {
                    transition.OnNext(view);
                })
                .Permit(Trigger.Next, UIViewType.End);

            machine.Configure(UIViewType.Clone)
                .OnEntry(() =>
                {
                    var view = SetupView(UIViewType.Clone);
                    transition.OnNext(view);
                })
                .Permit(Trigger.Next, UIViewType.End);

            machine.Configure(UIViewType.End)
                .OnEntry(() =>
                {
                    transition.OnCompleted();
                    transition.Dispose();
                })
                .Permit(Trigger.Next, UIViewType.Start);
        }

        IViewFor SetupView(UIViewType viewType)
        {
            IViewModel disposable;

            disposable = factory.GetViewModel(viewType);
            disposables.Add(disposable);
            var viewModel = disposable.Value;
            
            disposable = factory.GetView(viewType);
            disposables.Add(disposable);
            var view = disposable.Value;

            view.ViewModel = viewModel;
            return view;
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
