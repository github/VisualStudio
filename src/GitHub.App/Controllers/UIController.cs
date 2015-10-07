using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using GitHub.Authentication;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using Stateless;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace GitHub.Controllers
{
    [Export(typeof(IUIController))]
    public class UIController : IUIController, IDisposable
    {
        enum Trigger
        {
            Cancel = 0,
            Next,
            Detail,
            Creation,
            Finish
        }

        readonly IExportFactoryProvider factory;
        readonly IUIProvider uiProvider;
        readonly IRepositoryHosts hosts;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        readonly IConnectionManager connectionManager;

        readonly CompositeDisposable disposables = new CompositeDisposable();

        // holds state machines for each of the individual ui flows
        // does not load UI, merely tracks valid transitions
        readonly static Dictionary<UIControllerFlow, StateMachine<UIViewType, Trigger>> machines;
        readonly Dictionary<UIControllerFlow, Dictionary<UIViewType, UIPair>> uiObjects;

        // loads UI for each state corresponding to a view type
        // queries the individual ui flow (stored in machines) for the next state
        // for a given transition trigger
        readonly StateMachine<UIViewType, Trigger> uiStateMachine;

        Subject<IView> transition;

        // the main ui flow that this ui controller was set up to control
        UIControllerFlow mainFlow;

        // The currently active ui flow. This might be different from the main ui flow 
        // setup at the start if, for instance, the ui flow is tied
        // to a connection that requires being logged in. In this case, when loading
        // the ui, the active flow is switched to authentication until it's done, and then
        // back to the main ui flow. 
        UIControllerFlow activeFlow;
        NotifyCollectionChangedEventHandler connectionAdded;

        bool stopping;


        static UIController()
        {
            machines = new Dictionary<UIControllerFlow, StateMachine<UIViewType, Trigger>>();
            ConfigureStates();
        }

        [ImportingConstructor]
        public UIController(IUIProvider uiProvider, IRepositoryHosts hosts, IExportFactoryProvider factory,
            IConnectionManager connectionManager)
        {
            this.factory = factory;
            this.uiProvider = uiProvider;
            this.hosts = hosts;
            this.connectionManager = connectionManager;
            uiObjects = new Dictionary<UIControllerFlow, Dictionary<UIViewType, UIPair>>();

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
            uiStateMachine = new StateMachine<UIViewType, Trigger>(UIViewType.None);

            ConfigureUIHandlingStates();
        }

        /// <summary>
        /// Configures the UI that gets loaded when entering a certain state and which state
        /// to go to for each trigger. Which state to go to depends on which ui flow state machine
        /// is currently active - when a trigger happens, the PermitDynamic conditions will
        /// lookup the currently active state machine from the list of available ones in `machines`,
        /// fire the requested trigger on it, and return the state that it went to, which causes
        /// `uiStateMachine` to load a new UI (or exit)
        /// There is a bit of redundant information regarding valid state transitions between this
        /// state machine and the individual state machines for each ui flow. This is unavoidable
        /// because permited transition triggers have to be explicit, so care should be taken to
        /// make sure permitted triggers per view here match the permitted triggers in the individual
        /// state machines.
        /// </summary>
        void ConfigureUIHandlingStates()
        {
            uiStateMachine.Configure(UIViewType.None)
                .OnEntry(() => stopping = false)
                .PermitDynamic(Trigger.Next, () =>
                    {
                        var loggedIn = connection != null && hosts.LookupHost(connection.HostAddress).IsLoggedIn;
                        activeFlow = loggedIn ? mainFlow : UIControllerFlow.Authentication;
                        return Go(Trigger.Next);
                    }
                )
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Clone)
                .OnEntry(() => RunView(UIViewType.Clone))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Create)
                .OnEntry(() => RunView(UIViewType.Create))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Publish)
                .OnEntry(() => RunView(UIViewType.Publish))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.PRList)
                .OnEntry(() => RunView(UIViewType.PRList))
                .PermitDynamic(Trigger.Detail, () => Go(Trigger.Detail))
                .PermitDynamic(Trigger.Creation, () => Go(Trigger.Creation))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.PRDetail)
                .OnEntry(() => RunView(UIViewType.PRDetail))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.PRCreation)
                .OnEntry(() => RunView(UIViewType.PRCreation))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Login)
                .OnEntry(() => RunView(UIViewType.Login))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.TwoFactor)
                .OnEntry(() => RunView(UIViewType.TwoFactor))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.End)
                .OnEntry(() =>
                {
                    Dictionary<UIViewType, UIPair> list = GetObjectsForFlow(activeFlow);
                    foreach (var i in list.Values)
                        i.ClearHandlers();

                    if (activeFlow == mainFlow)
                        transition.OnCompleted();

                    // if we're stopping the controller and the active flow wasn't the main one
                    // we need to stop the main flow
                    else if (stopping)
                        Fire(Trigger.Finish);
                    else
                        Fire(Trigger.Next);
                })
                // clear all the views and viewmodels created by a subflow
                .OnExit(() =>
                {
                    Dictionary<UIViewType, UIPair> list = GetObjectsForFlow(activeFlow);
                    foreach (var i in list.Values)
                        i.Dispose();
                    list.Clear();

                    var loggedIn = connection != null && hosts.LookupHost(connection.HostAddress).IsLoggedIn;
                    activeFlow = loggedIn ? mainFlow : UIControllerFlow.Authentication;
                })
                .PermitDynamic(Trigger.Next, () =>
                    {
                        var state = Go(Trigger.Next);
                        if (activeFlow != mainFlow)
                        {
                            var loggedIn = connection != null && hosts.LookupHost(connection.HostAddress).IsLoggedIn;
                            state = Go(Trigger.Next, loggedIn ? mainFlow : UIControllerFlow.Authentication);
                        }
                        return state;
                    }
                )
                .Permit(Trigger.Finish, UIViewType.None);
        }

        Dictionary<UIViewType, UIPair> GetObjectsForFlow(UIControllerFlow flow)
        {
            Dictionary<UIViewType, UIPair> list;
            if (!uiObjects.TryGetValue(flow, out list))
            {
                list = new Dictionary<UIViewType, UIPair>();
                uiObjects.Add(flow, list);
            }
            return list;
        }

        /// <summary>
        /// Configure all the logical state transitions for each of the
        /// ui flows we support.
        /// </summary>
        static void ConfigureStates()
        {
            StateMachine<UIViewType, Trigger> logic;

            // authentication flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.Login)
                .Permit(Trigger.Cancel, UIViewType.End);
            logic.Configure(UIViewType.Login)
                .Permit(Trigger.Next, UIViewType.TwoFactor)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.TwoFactor)
                .Permit(Trigger.Next, UIViewType.End)
                .Permit(Trigger.Cancel, UIViewType.Login)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);

            machines.Add(UIControllerFlow.Authentication, logic);

            // clone flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.Clone)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.Clone)
                .Permit(Trigger.Next, UIViewType.End)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(UIControllerFlow.Clone, logic);

            // create flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.Create)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.Create)
                .Permit(Trigger.Next, UIViewType.End)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(UIControllerFlow.Create, logic);

            // publish flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.Publish);
            logic.Configure(UIViewType.Publish)
                .Permit(Trigger.Next, UIViewType.End)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(UIControllerFlow.Publish, logic);

            // pr flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.PRList)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.PRList)
                .Permit(Trigger.Detail, UIViewType.PRDetail)
                .Permit(Trigger.Creation, UIViewType.PRCreation)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.PRDetail)
                .Permit(Trigger.Next, UIViewType.PRList)
                .Permit(Trigger.Cancel, UIViewType.PRList)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.PRCreation)
                .Permit(Trigger.Next, UIViewType.PRList)
                .Permit(Trigger.Cancel, UIViewType.PRList)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(UIControllerFlow.PullRequests, logic);
        }

        public IObservable<IView> SelectFlow(UIControllerFlow choice)
        {
            mainFlow = choice;

            transition = new Subject<IView>();
            transition.Subscribe(_ => {}, _ => Fire(Trigger.Next));
        
            return transition;
        }

        public void Stop()
        {
            Debug.WriteLine("Stop ({0})", GetHashCode());
            stopping = true;
            Fire(Trigger.Finish);
        }

        void RunView(UIViewType viewType)
        {
            var view = CreateViewAndViewModel(viewType);
            transition.OnNext(view);

            // controller might have been stopped in the OnNext above
            if (IsStopped)
                return;

            SetupView(viewType, view);
        }

        void SetupView(UIViewType viewType, IView view)
        {
            var list = GetObjectsForFlow(activeFlow);
            var pair = list[viewType];
            // we're setting up the login dialog, we need to setup the 2fa as
            // well to continue the flow if it's needed, since the
            // authenticationresult callback won't happen until
            // everything is done
            if (viewType == UIViewType.Login)
            {
                var pair2fa = list[UIViewType.TwoFactor];
                var twofa = pair2fa.ViewModel;
                pair2fa.AddHandler(twofa.WhenAny(x => x.IsShowing, x => x.Value)
                    .Where(x => x)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Next)));

                pair.AddHandler(view.Done
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Finish)));
            }

            // the 2fa dialog is special, it's setup when the login view is setup (see above)
            else if (viewType != UIViewType.TwoFactor)
            {
                pair.AddHandler(view.Done
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Next)));

                var cv = view as IHasCreationView;
                if (cv != null)
                    pair.AddHandler(cv.Create
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ => Fire(Trigger.Creation)));

                var dv = view as IHasDetailView;
                if (dv != null)
                    pair.AddHandler(dv.Open
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ => Fire(Trigger.Detail)));
            }
            pair.AddHandler(view.Cancel.Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Cancel) ? Trigger.Cancel : Trigger.Finish)));
        }

        IView CreateViewAndViewModel(UIViewType viewType)
        {
            var list = GetObjectsForFlow(activeFlow);
            if (viewType == UIViewType.Login)
            {
                if (!list.ContainsKey(viewType))
                {
                    var d = new UIPair(UIViewType.TwoFactor, factory.GetView(UIViewType.TwoFactor), factory.GetViewModel(UIViewType.TwoFactor));
                    list.Add(UIViewType.TwoFactor, d);
                }
            }

            // 2fa view/viewmodel is created when login is created 'cause login needs the 2fa viewmodel
            // so the only thing we want to do is connect the viewmodel to the view when it's showing
            else if (viewType == UIViewType.TwoFactor)
            {
                var d = list[viewType];
                if (d.View.ViewModel == null)
                    d.View.ViewModel = d.ViewModel;
            }

            if (!list.ContainsKey(viewType))
            {
                var d = new UIPair(viewType, factory.GetView(viewType), factory.GetViewModel(viewType));
                d.View.ViewModel = d.ViewModel;
                list.Add(viewType, d);
            }

            return list[viewType].View;
        }

        void Fire(Trigger next)
        {
            Debug.WriteLine("Firing {0} ({1})", next, GetHashCode());
            uiStateMachine.Fire(next);
        }

        UIViewType Go(Trigger trigger)
        {
            return Go(trigger, activeFlow);
        }

        UIViewType Go(Trigger trigger, UIControllerFlow flow)
        {
            Debug.WriteLine("Firing {0} for flow {1} ({2})", trigger, flow, GetHashCode());
            var m = machines[flow];
            m.Fire(trigger);
            return m.State;
        }

        IConnection connection;
        public void Start([AllowNull] IConnection conn)
        {
            connection = conn;
            if (connection != null)
            {
                uiProvider.AddService(typeof(IConnection), connection);
                connection.Login()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => { }, () =>
                    {
                        Debug.WriteLine("Start ({0})", GetHashCode());
                        Fire(Trigger.Next);
                    });
            }
            else
            {
                connectionManager
                    .IsLoggedIn(hosts)
                    .Do(loggedIn =>
                    {
                        if (!loggedIn && mainFlow != UIControllerFlow.Authentication)
                        {
                            connectionAdded = (s, e) => {
                                if (e.Action == NotifyCollectionChangedAction.Add)
                                {
                                    connection = (IConnection)e.NewItems[0];
                                    uiProvider.AddService(typeof(IConnection), connection);
                                }
                            };
                            connectionManager.Connections.CollectionChanged += connectionAdded;
                        }
                    })
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => { }, () =>
                    {
                        Debug.WriteLine("Start ({0})", GetHashCode());
                        Fire(Trigger.Next);
                    });
            }
        }

        bool disposed; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                Debug.WriteLine("Disposing ({0})", GetHashCode());
                disposables.Dispose();
                transition?.Dispose();
                if (connectionAdded != null)
                    connectionManager.Connections.CollectionChanged -= connectionAdded;
                connectionAdded = null;
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsStopped => uiStateMachine.IsInState(UIViewType.None) || stopping;

        class UIPair : IDisposable
        {
            ExportLifetimeContext<IView> view;
            ExportLifetimeContext<IViewModel> viewModel;
            CompositeDisposable handlers = new CompositeDisposable();
            UIViewType viewType;

            public UIViewType ViewType => viewType;
            public IView View => view.Value;
            public IViewModel ViewModel => viewModel?.Value;

            public UIPair(UIViewType type, ExportLifetimeContext<IView> v, [AllowNull]ExportLifetimeContext<IViewModel> vm)
            {
                viewType = type;
                view = v;
                viewModel = vm;
                handlers = new CompositeDisposable();
            }

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
                    view = null;
                    viewModel?.Dispose();
                    viewModel = null;
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
}
