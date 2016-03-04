using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
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
    /*
        This class creates views and associated viewmodels for each UI we have,
        and controls the UI logic graph. It uses various state machines to define
        the UI logic graph.

        **State machines**

        The UI logic graph is controlled by various state machines defined in 
        `ConfigureLogicStates` and `ConfigureUIHandlingStates`

        **ConfigureLogicStates**
        
        `ConfigureLogicStates` defines the one state machine per UI group:
            - Authentication
            - Repository Clone
            - Repository Creation
            - Repository Publish,
            - Pull Requests (List, Detail, Creation)

        All state machines have a common state of `None` (nothing happened yet),
        `End` (we're done, cleanup) and `Finish` (final state once cleanup is done), and
        UI-specific states that relate to what views the UI needs to show (Login, TwoFactor,
        PullRequestList, etc). States are defined in the enum `UIViewType`

        All state machines support a variety of triggers for going from one state to another.
        These triggers are defined in the enum `Trigger`. `Cancel` and `Finish` are
        supported by all states (any state machine regardless of its current state supports
        exiting via `Cancel` (ends with success flag set to false) and `Finish` (ends with success flag set to true).
        Since most UI flows we support are linear (login followed by 2fa followed by clone
        followed by ending the flow), most states support the `Next` trigger to continue,
        and when at the end of a ui flow, `Next` ends it with success.

        The Pull Requests UI flow is non-linear (there are more than one transition from the
        list view - it can go to the detail view, or the pr creation view, or other views),
        it does not support the `Next` trigger and instead has its own triggers.

        **ConfigureUIHandlingStates**
        
        `ConfigureUIHandlingStates` defines a state machine `uiStateMachine` connected
        to the `transition` observable, and executes whatever logic is needed to load the
        requested state, usually by creating the view and viewmodel that the UI state requires.
        Whenever a state transition happens in `uiStateMachine`, if the state requires
        loading a view, this view is sent back to the rendering code via the `transition`
        observable. When the state machine reaches the `End` state, the `completion` observable
        is completed with a flag indicating whether the ui flow ended successfully or was
        cancelled (via the `Next`/`Finish` triggers or the `Cancel` trigger) .

        The transitions between states are dynamically evaluated at runtime based on the logic
        defined in `ConfigureLogicStates`. When `uiStateMachine` receives a trigger request,
        it passes that trigger to the state machine corresponding to the ui flow that's currently
        running (Authentication, Clone, etc), and that state machine determines which state
        to go to next.
        
        In theory, `uiStateMachine` has no knowledge of the logic graph, and
        the only thing it's responsible for is loading UI whenever a state is entered, and cleaning
        up objects when things are done - making it essentially a big switch with nice entry and exit
        conditions. In practice, we need to configure the valid triggers from one state to the other
        just so we can call the corresponding state machine that handles the logic (defined in `ConfigureLogicStates`).
        There is a bit of code duplication because of this, and there's some room for improvement
        here, but because we have a small number of triggers, it's not a huge deal.
    */

    [Export(typeof(IUIController))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
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
        readonly Dictionary<UIControllerFlow, StateMachine<UIViewType, Trigger>> machines;
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

        Subject<bool> completion;

        bool stopping;

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
            machines = new Dictionary<UIControllerFlow, StateMachine<UIViewType, Trigger>>();
            ConfigureLogicStates();

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
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Create)
                .OnEntry(() => RunView(UIViewType.Create))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Publish)
                .OnEntry(() => RunView(UIViewType.Publish))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.PRList)
                .OnEntry(() => RunView(UIViewType.PRList))
                .PermitDynamic(Trigger.Detail, () => Go(Trigger.Detail))
                .PermitDynamic(Trigger.Creation, () => Go(Trigger.Creation))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
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
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.TwoFactor)
                .OnEntry(() => RunView(UIViewType.TwoFactor))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.End)
                .OnEntryFrom(Trigger.Cancel, () => End(false))
                .OnEntryFrom(Trigger.Next, () => End(true))
                .OnEntryFrom(Trigger.Finish, () => End(true))
                // clear all the views and viewmodels created by a subflow
                .OnExit(() =>
                {
                    var list = GetObjectsForFlow(activeFlow);
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

        /// <summary>
        /// Configure all the logical state transitions for each of the
        /// ui flows we support.
        /// </summary>
        void ConfigureLogicStates()
        {
            StateMachine<UIViewType, Trigger> logic;

            // no selected flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Ignore(Trigger.Next)
                .Ignore(Trigger.Finish);
            machines.Add(UIControllerFlow.None, logic);

            // authentication flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.Login)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.Login)
                .Permit(Trigger.Next, UIViewType.TwoFactor)
                .Permit(Trigger.Cancel, UIViewType.End)
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
                .Permit(Trigger.Cancel, UIViewType.End)
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
                .Permit(Trigger.Cancel, UIViewType.End)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(UIControllerFlow.Create, logic);

            // publish flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.Publish)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.Publish)
                .Permit(Trigger.Next, UIViewType.End)
                .Permit(Trigger.Cancel, UIViewType.End)
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
                .Permit(Trigger.Cancel, UIViewType.End)
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

        /// <summary>
        /// Allows listening to the completion state of the ui flow - whether
        /// it was completed because it was cancelled or whether it succeeded.
        /// </summary>
        /// <returns>true for success, false for cancel</returns>
        public IObservable<bool> ListenToCompletionState()
        {
            if (completion == null)
                completion = new Subject<bool>();
            return completion;
        }

        void End(bool success)
        {
            var list = GetObjectsForFlow(activeFlow);
            foreach (var i in list.Values)
                i.ClearHandlers();

            if (activeFlow == mainFlow)
            {
                uiProvider.RemoveService(typeof(IConnection));
                completion?.OnNext(success);
                completion?.OnCompleted();
                transition.OnCompleted();
            }
            // if we're stopping the controller and the active flow wasn't the main one
            // we need to stop the main flow
            else if (stopping)
                Fire(Trigger.Finish);
            else
                Fire(Trigger.Next);

        }

        public void Stop()
        {
            Debug.WriteLine("Stopping {0} ({1})", activeFlow, GetHashCode());
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
                    .Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Next) ? Trigger.Next : Trigger.Finish)));

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
            pair.AddHandler(view.Cancel
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Cancel) ? Trigger.Cancel : Trigger.Finish)));
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


        /// <summary>
        /// Returns the view/viewmodel pair for a given flow
        /// </summary>
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

        void Fire(Trigger next)
        {
            Debug.WriteLine("Firing {0} from {1} ({2})", next, uiStateMachine.State, GetHashCode());
            uiStateMachine.Fire(next);
        }

        UIViewType Go(Trigger trigger)
        {
            return Go(trigger, activeFlow);
        }

        UIViewType Go(Trigger trigger, UIControllerFlow flow)
        {
            var m = machines[flow];
            Debug.WriteLine("Firing {0} from {1} for flow {2} ({3})", trigger, m.State, flow, GetHashCode());
            m.Fire(trigger);
            return m.State;
        }

        IConnection connection;
        public void Start([AllowNull] IConnection conn)
        {
            connection = conn;
            if (connection != null)
            {
                if (mainFlow != UIControllerFlow.Authentication)
                    uiProvider.AddService(connection);
                else // sanity check: it makes zero sense to pass a connection in when calling the auth flow
                    Debug.Assert(false, "Calling the auth flow with a connection makes no sense!");

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
                    .GetLoggedInConnections(hosts)
                    .FirstOrDefaultAsync()
                    .Select(c =>
                    {
                        bool loggedin = c != null;
                        if (mainFlow != UIControllerFlow.Authentication)
                        {
                            if (loggedin) // register the first available connection so the viewmodel can use it
                            {
                                connection = c;
                                uiProvider.AddService(c);
                            }
                            else
                            {
                                // a connection will be added to the list when auth is done, register it so the next
                                // viewmodel can use it
                                connectionAdded = (s, e) =>
                                {
                                    if (e.Action == NotifyCollectionChangedAction.Add)
                                    {
                                        connection = e.NewItems[0] as IConnection;
                                        uiProvider.AddService(typeof(IConnection), connection);
                                    }
                                };
                                connectionManager.Connections.CollectionChanged += connectionAdded;
                            }
                        }
                        return loggedin;
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
                disposed = true;

                Debug.WriteLine("Disposing ({0})", GetHashCode());

                if (connectionAdded != null)
                    connectionManager.Connections.CollectionChanged -= connectionAdded;
                connectionAdded = null;

                var tr = transition;
                var cmp = completion;
                transition = null;
                completion = null;
                disposables.Dispose();
                tr?.Dispose();
                cmp?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsStopped => uiStateMachine.IsInState(UIViewType.None) || stopping;
        public UIControllerFlow CurrentFlow => activeFlow;

        /// <summary>
        /// This class holds ExportLifetimeContexts (i.e., Lazy Disposable containers) for IView and IViewModel objects
        /// A view type (login, clone, etc) is composed of a pair of view and viewmodel, which this class represents.
        /// </summary>
        class UIPair : IDisposable
        {
            ExportLifetimeContext<IView> view;
            ExportLifetimeContext<IViewModel> viewModel;
            CompositeDisposable handlers = new CompositeDisposable();
            UIViewType viewType;

            public UIViewType ViewType => viewType;
            public IView View => view.Value;
            public IViewModel ViewModel => viewModel?.Value;

            /// <param name="type">The UIViewType</param>
            /// <param name="v">The IView</param>
            /// <param name="vm">The IViewModel. Might be null because the 2fa view shares the same viewmodel as the login dialog, so it's
            /// set manually in the view outside of this</param>
            public UIPair(UIViewType type, ExportLifetimeContext<IView> v, [AllowNull]ExportLifetimeContext<IViewModel> vm)
            {
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
