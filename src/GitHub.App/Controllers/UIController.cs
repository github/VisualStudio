using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using ReactiveUI;
using Stateless;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows;

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

    using App.Factories;
    using ViewModels;
    using StateMachineType = StateMachine<UIViewType, UIController.Trigger>;

    public class UIController : IUIController
    {
        internal enum Trigger
        {
            None,
            Cancel,
            Next,
            Reload,
            Finish
        }

        readonly IUIFactory factory;
        readonly IGitHubServiceProvider gitHubServiceProvider;
        readonly IRepositoryHosts hosts;
        readonly IConnectionManager connectionManager;

        readonly CompositeDisposable disposables = new CompositeDisposable();

        // holds state machines for each of the individual ui flows
        // does not load UI, merely tracks valid transitions
        readonly Dictionary<UIControllerFlow, StateMachineType> machines =
             new Dictionary<UIControllerFlow, StateMachineType>();

        readonly Dictionary<UIControllerFlow, Dictionary<UIViewType, IUIPair>> uiObjects =
             new Dictionary<UIControllerFlow, Dictionary<UIViewType, IUIPair>>();

        // loads UI for each state corresponding to a view type
        // queries the individual ui flow (stored in machines) for the next state
        // for a given transition trigger
        readonly StateMachineType uiStateMachine;

        readonly Dictionary<Trigger, StateMachineType.TriggerWithParameters<ViewWithData>> triggers =
             new Dictionary<Trigger, StateMachineType.TriggerWithParameters<ViewWithData>>();

        Subject<LoadData> transition;
        public IObservable<LoadData> TransitionSignal => transition;

        // the main ui flow that this ui controller was set up to control
        UIControllerFlow selectedFlow;

        // The currently active ui flow. This might be different from the main ui flow 
        // setup at the start if, for instance, the ui flow is tied
        // to a connection that requires being logged in. In this case, when loading
        // the ui, the active flow is switched to authentication until it's done, and then
        // back to the main ui flow. 
        UIControllerFlow activeFlow;
        NotifyCollectionChangedEventHandler connectionAdded;

        Subject<bool> completion;
        IConnection connection;
        ViewWithData requestedTarget;
        bool stopping;

        public UIController(IGitHubServiceProvider serviceProvider)
            : this(serviceProvider,
                   serviceProvider.TryGetService<IRepositoryHosts>(),
                   serviceProvider.TryGetService<IUIFactory>(),
                   serviceProvider.TryGetService<IConnectionManager>())
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public UIController(IGitHubServiceProvider gitHubServiceProvider,
            IRepositoryHosts hosts, IUIFactory factory,
            IConnectionManager connectionManager)
        {
            Guard.ArgumentNotNull(gitHubServiceProvider, nameof(gitHubServiceProvider));
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            Guard.ArgumentNotNull(factory, nameof(factory));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));

            this.factory = factory;
            this.gitHubServiceProvider = gitHubServiceProvider;
            this.hosts = hosts;
            this.connectionManager = connectionManager;

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
            ConfigureLogicStates();

            uiStateMachine = new StateMachineType(UIViewType.None);

            ConfigureUIHandlingStates();

        }

        public IObservable<LoadData> Configure(UIControllerFlow choice,
            IConnection conn = null,
            ViewWithData parameters = null)
        {
            connection = conn;
            selectedFlow = choice;
            requestedTarget = parameters;

            transition = new Subject<LoadData>();
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

        public void Start()
        {
            if (connection != null)
            {
                if (selectedFlow != UIControllerFlow.Authentication)
                    gitHubServiceProvider.AddService(this, connection);
                else // sanity check: it makes zero sense to pass a connection in when calling the auth flow
                    Debug.Assert(false, "Calling the auth flow with a connection makes no sense!");

                hosts.EnsureInitialized().ToObservable()
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
                        if (selectedFlow != UIControllerFlow.Authentication)
                        {
                            if (loggedin) // register the first available connection so the viewmodel can use it
                            {
                                connection = c;
                                gitHubServiceProvider.AddService(this, c);
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
                                        if (connection != null)
                                            gitHubServiceProvider.AddService(typeof(IConnection), this, connection);
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

        public void Stop()
        {
            if (stopping || transition == null)
                return;

            Debug.WriteLine("Stopping {0} ({1})", activeFlow + (activeFlow != selectedFlow ? " and " + selectedFlow : ""), GetHashCode());
            stopping = true;
            Fire(Trigger.Finish);
        }

        public void Reload()
        {
            Fire(Trigger.Reload);
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
            triggers.Add(Trigger.Next, uiStateMachine.SetTriggerParameters<ViewWithData>(Trigger.Next));

            uiStateMachine.Configure(UIViewType.None)
                .OnEntry(tr => stopping = false)
                .PermitDynamic(Trigger.Next, () =>
                    {
                        activeFlow = SelectActiveFlow();
                        return Go(Trigger.Next);
                    })
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            ConfigureEntriesExitsForView(UIViewType.Clone);
            ConfigureEntriesExitsForView(UIViewType.Create);
            ConfigureEntriesExitsForView(UIViewType.Publish);
            ConfigureEntriesExitsForView(UIViewType.PRList);
            ConfigureEntriesExitsForView(UIViewType.PRDetail);
            ConfigureEntriesExitsForView(UIViewType.PRCreation);
            ConfigureEntriesExitsForView(UIViewType.Login);
            ConfigureEntriesExitsForView(UIViewType.TwoFactor);
            ConfigureEntriesExitsForView(UIViewType.Gist);
            ConfigureEntriesExitsForView(UIViewType.StartPageClone);

            uiStateMachine.Configure(UIViewType.End)
                .OnEntryFrom(Trigger.Cancel, () => End(false))
                .OnEntryFrom(Trigger.Next, () => End(true))
                .OnEntryFrom(Trigger.Finish, () => End(true))
                // clear all the views and viewmodels created by a subflow
                .OnExit(() =>
                {
                    // it's important to have the stopping flag set before we do this
                    if (activeFlow == selectedFlow)
                    {
                        completion?.OnNext(Success.Value);
                        completion?.OnCompleted();
                    }

                    DisposeFlow(activeFlow);

                    if (activeFlow == selectedFlow)
                    {
                        gitHubServiceProvider.RemoveService(typeof(IConnection), this);
                        transition.OnCompleted();
                        Reset();
                    }
                    else
                        activeFlow = stopping || LoggedIn ? selectedFlow : UIControllerFlow.Authentication;
                })
                .PermitDynamic(Trigger.Next, () =>
                {
                    // sets the state to None for the current flow
                    var state = Go(Trigger.Next);

                    if (activeFlow != selectedFlow)
                    {
                        if (stopping)
                        {
                            // triggering the End state again so that it can clear up the main flow and really end
                            state = Go(Trigger.Finish, selectedFlow);
                        }
                        else
                        {
                            // sets the state to the first UI of the main flow or the auth flow, depending on whether you're logged in
                            state = Go(Trigger.Next, LoggedIn ? selectedFlow : UIControllerFlow.Authentication);
                        }
                    }
                    return state;
                })
                .Permit(Trigger.Finish, UIViewType.None);
        }

        StateMachineType.StateConfiguration ConfigureEntriesExitsForView(UIViewType viewType)
        {
            return uiStateMachine.Configure(viewType)
                .OnEntryFrom(triggers[Trigger.Next], (arg, tr) => RunView(tr.Destination, arg))
                .OnEntry(tr => {
                    // Trigger.Next is always called in OnEntryFrom, don't want to run it twice
                    if (tr.Trigger != Trigger.Next) RunView(tr.Destination);
                })
                .PermitReentry(Trigger.Reload)
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));
        }

        /// <summary>
        /// Configure all the logical state transitions for each of the
        /// ui flows we support that have more than one view
        /// </summary>
        void ConfigureLogicStates()
        {
            StateMachineType logic;

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

            ConfigureSingleViewLogic(UIControllerFlow.Clone, UIViewType.Clone);
            ConfigureSingleViewLogic(UIControllerFlow.Create, UIViewType.Create);
            ConfigureSingleViewLogic(UIControllerFlow.Gist, UIViewType.Gist);
            ConfigureSingleViewLogic(UIControllerFlow.Home, UIViewType.PRList);
            ConfigureSingleViewLogic(UIControllerFlow.Publish, UIViewType.Publish);
            ConfigureSingleViewLogic(UIControllerFlow.PullRequestList, UIViewType.PRList);
            ConfigureSingleViewLogic(UIControllerFlow.PullRequestDetail, UIViewType.PRDetail);
            ConfigureSingleViewLogic(UIControllerFlow.PullRequestCreation, UIViewType.PRCreation);
            ConfigureSingleViewLogic(UIControllerFlow.ReClone, UIViewType.StartPageClone);
        }

        void ConfigureSingleViewLogic(UIControllerFlow flow, UIViewType type)
        {
            var logic = new StateMachineType(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, type)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(type)
                .Permit(Trigger.Next, UIViewType.End)
                .Permit(Trigger.Cancel, UIViewType.End)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(flow, logic);
        }

        UIControllerFlow SelectActiveFlow()
        {
            var loggedIn = connection?.IsLoggedIn ?? false;
            return loggedIn ? selectedFlow : UIControllerFlow.Authentication;
        }

        /// <summary>
        /// End state for a flow has been called. Clear handlers related to that flow
        /// and call completion handlers if this means the controller is being stopped.
        /// Handlers registered via `ListenToCompletionState` might need to access the
        /// view/viewmodel objects, so those can only be disposed when we're leaving
        /// the End state, not here.
        /// </summary>
        /// <param name="success"></param>
        void End(bool success)
        {
            if (!Success.HasValue)
                Success = success;

            ShutdownFlow(activeFlow);

            // if the auth was cancelled, we need to stop everything, otherwise we'll go into a loop
            if (activeFlow == selectedFlow || !Success.Value)
                stopping = true;

            Fire(Trigger.Next);
        }

        void ShutdownFlow(UIControllerFlow flow)
        {
            var list = GetObjectsForFlow(flow);
            foreach (var i in list.Values)
                i.ClearHandlers();
        }

        void DisposeFlow(UIControllerFlow flow)
        {
            var list = GetObjectsForFlow(flow);
            foreach (var i in list.Values)
                i.Dispose();
            list.Clear();
        }

        void DisposeView(UIControllerFlow flow, UIViewType type)
        {
            var list = GetObjectsForFlow(flow);
            IUIPair uipair = null;
            if (list.TryGetValue(type, out uipair))
            {
                list.Remove(type);
                uipair.Dispose();
            }
        }

        void RunView(UIViewType viewType, ViewWithData arg = null)
        {
            if (requestedTarget?.ViewType == viewType || (requestedTarget?.ViewType == UIViewType.None && requestedTarget?.MainFlow == CurrentFlow))
            {
                arg = requestedTarget;
            }

            if (arg == null)
                arg = new ViewWithData { ActiveFlow = activeFlow, MainFlow = selectedFlow, ViewType = viewType };
            bool firstTime = CreateViewAndViewModel(viewType, arg);
            var view = GetObjectsForFlow(activeFlow)[viewType].View;
            transition.OnNext(new LoadData
            {
                Flow = activeFlow,
                View = view,
                Data = arg
            });

            // controller might have been stopped in the OnNext above
            if (IsStopped)
                return;

            // if it's not the first time we've shown this view, no need
            // to set it up
            if (!firstTime)
                return;

            SetupView(viewType, view.ViewModel);
        }

        void SetupView(UIViewType viewType, IViewModel viewModel)
        {
            var list = GetObjectsForFlow(activeFlow);
            var pair = list[viewType];
            var hasDone = viewModel as IHasDone;
            var hasCancel = viewModel as IHasCancel;

            // 2FA is set up when login is set up, so nothing to do
            if (viewType == UIViewType.TwoFactor)
                return;

            // we're setting up the login dialog, we need to setup the 2fa as
            // well to continue the flow if it's needed, since the
            // authenticationresult callback won't happen until
            // everything is done
            if (viewType == UIViewType.Login)
            {
                var pair2fa = list[UIViewType.TwoFactor];
                pair2fa.AddHandler(((IDialogViewModel)pair2fa.ViewModel).WhenAny(x => x.IsShowing, x => x.Value)
                    .Where(x => x)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Next)));

                pair2fa.AddHandler(((IHasCancel)pair2fa.ViewModel).Cancel
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Cancel) ? Trigger.Cancel : Trigger.Finish)));

                if (hasDone != null)
                {
                    pair.AddHandler(hasDone.Done
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ => Fire(Trigger.Finish)));
                }
            }
            else if (hasDone != null)
            {
                pair.AddHandler(hasDone.Done
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Next) ? Trigger.Next : Trigger.Finish)));
            }

            if (hasCancel != null)
            {
                pair.AddHandler(hasCancel.Cancel
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Cancel) ? Trigger.Cancel : Trigger.Finish)));
            }
        }

        /// <summary>
        /// Creates View/ViewModel instances for the specified <paramref name="viewType"/> if they
        /// haven't been created yet in the current flow
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns>true if the View/ViewModel didn't exist and had to be created</returns>
        bool CreateViewAndViewModel(UIViewType viewType, ViewWithData data = null)
        {
            var list = GetObjectsForFlow(activeFlow);
            if (viewType == UIViewType.Login)
            {
                if (!list.ContainsKey(viewType))
                {
                    var d = factory.CreateViewAndViewModel(UIViewType.TwoFactor);
                    list.Add(UIViewType.TwoFactor, d);
                }
            }

            // 2fa view/viewmodel is created when login is created 'cause login needs the 2fa viewmodel
            // so the only thing we want to do is connect the viewmodel to the view when it's showing
            else if (viewType == UIViewType.TwoFactor)
            {
                var d = list[viewType];
                if (d.View.ViewModel == null)
                {
                    d.ViewModel.Initialize(data);
                    d.View.DataContext = d.ViewModel;
                }
            }

            IUIPair pair = null;
            var firstTime = !list.TryGetValue(viewType, out pair);

            if (firstTime)
                pair = factory.CreateViewAndViewModel(viewType);

            pair.ViewModel.Initialize(data);

            if (firstTime)
            {
                pair.View.DataContext = pair.ViewModel;
                list.Add(viewType, pair);
            }

            return firstTime;
        }


        /// <summary>
        /// Returns the view/viewmodel pair for a given flow
        /// </summary>
        Dictionary<UIViewType, IUIPair> GetObjectsForFlow(UIControllerFlow flow)
        {
            Dictionary<UIViewType, IUIPair> list;
            if (!uiObjects.TryGetValue(flow, out list))
            {
                list = new Dictionary<UIViewType, IUIPair>();
                uiObjects.Add(flow, list);
            }
            return list;
        }

        void Fire(Trigger next, ViewWithData arg = null)
        {
            Debug.WriteLine("Firing {0} from {1} ({2})", next, uiStateMachine.State, GetHashCode());
            if (triggers.ContainsKey(next))
                uiStateMachine.Fire(triggers[next], arg);
            else
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

        void Reset()
        {
            if (connectionAdded != null)
                connectionManager.Connections.CollectionChanged -= connectionAdded;
            connectionAdded = null;
            
            var tr = transition;
            var cmp = completion;
            transition = null;
            completion = null;
            disposables.Clear();
            tr?.Dispose();
            cmp?.Dispose();
            stopping = false;
            connection = null;
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
        public UIControllerFlow SelectedFlow => selectedFlow;
        bool LoggedIn => connection?.IsLoggedIn ?? false;
        bool? Success { get; set; }
    }
}
