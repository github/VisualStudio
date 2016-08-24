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
using NullGuard;
using ReactiveUI;
using Stateless;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.Controllers
{
    using App.Factories;
    using System.Globalization;
    using System.Windows.Controls;
    using StateMachineType = StateMachine<UIViewType, UIController.Trigger>;

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
        internal enum Trigger
        {
            None,
            Cancel,
            Next,
            PRList,
            PRDetail,
            PRCreation,
            Finish
        }

        readonly IUIFactory factory;
        readonly IUIProvider uiProvider;
        readonly IRepositoryHosts hosts;
        readonly IConnectionManager connectionManager;

        readonly CompositeDisposable disposables = new CompositeDisposable();

        // holds state machines for each of the individual ui flows
        // does not load UI, merely tracks valid transitions
        readonly Dictionary<UIControllerFlow, StateMachine<UIViewType, Trigger>> machines;
        readonly Dictionary<UIControllerFlow, Dictionary<UIViewType, IUIPair>> uiObjects;

        // loads UI for each state corresponding to a view type
        // queries the individual ui flow (stored in machines) for the next state
        // for a given transition trigger
        readonly StateMachineType uiStateMachine;
        readonly Dictionary<Trigger, StateMachineType.TriggerWithParameters<ViewWithData>> triggers;

        Subject<LoadData> transition;

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
        IConnection connection;
        ViewWithData requestedTarget;
        bool stopping;

        [ImportingConstructor]
        public UIController(IUIProvider uiProvider, IRepositoryHosts hosts, IUIFactory factory,
            IConnectionManager connectionManager)
        {
            this.factory = factory;
            this.uiProvider = uiProvider;
            this.hosts = hosts;
            this.connectionManager = connectionManager;
            uiObjects = new Dictionary<UIControllerFlow, Dictionary<UIViewType, IUIPair>>();

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

            uiStateMachine = new StateMachineType(UIViewType.None);
            triggers = new Dictionary<Trigger, StateMachineType.TriggerWithParameters<ViewWithData>>();

            ConfigureUIHandlingStates();

        }

        public IObservable<LoadData> SelectFlow(UIControllerFlow choice)
        {
            mainFlow = choice;

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

        public void Start([AllowNull] IConnection conn)
        {
            connection = conn;
            if (connection != null)
            {
                if (mainFlow != UIControllerFlow.Authentication)
                    uiProvider.AddService(this, connection);
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
                                uiProvider.AddService(this, c);
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
                                            uiProvider.AddService(typeof(IConnection), this, connection);
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

        public void Jump(ViewWithData where)
        {
            Debug.Assert(where.ActiveFlow == mainFlow, "Jump called for flow " + where.ActiveFlow + " but this is " + mainFlow);
            if (where.ActiveFlow != mainFlow)
                return;

            requestedTarget = where;
            if (activeFlow == where.ActiveFlow)
                Fire(Trigger.Next, where);
        }

        public void Stop()
        {
            if (stopping || transition == null)
                return;

            Debug.WriteLine("Stopping {0} ({1})", activeFlow + (activeFlow != mainFlow ? " and " + mainFlow : ""), GetHashCode());
            stopping = true;
            Fire(Trigger.Finish);
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
                .OnEntry(tr => stopping = false)
                .PermitDynamic(Trigger.Next, () =>
                    {
                        activeFlow = SelectActiveFlow();
                        return Go(Trigger.Next);
                    })
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Clone)
                .OnEntry(tr => RunView(UIViewType.Clone, CalculateDirection(tr)))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Create)
                .OnEntry(tr => RunView(UIViewType.Create, CalculateDirection(tr)))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Publish)
                .OnEntry(tr => RunView(UIViewType.Publish, CalculateDirection(tr)))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.PRList)
                .OnEntry(tr => RunView(UIViewType.PRList, CalculateDirection(tr)))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.PRDetail, () => Go(Trigger.PRDetail))
                .PermitDynamic(Trigger.PRCreation, () => Go(Trigger.PRCreation))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            triggers.Add(Trigger.PRDetail, uiStateMachine.SetTriggerParameters<ViewWithData>(Trigger.PRDetail));
            uiStateMachine.Configure(UIViewType.PRDetail)
                .OnEntryFrom(triggers[Trigger.PRDetail], (arg, tr) => RunView(UIViewType.PRDetail, CalculateDirection(tr), arg))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish))
                .OnExit(() => DisposeView(activeFlow, UIViewType.PRDetail));

            uiStateMachine.Configure(UIViewType.PRCreation)
                .OnEntry(tr => RunView(UIViewType.PRCreation, CalculateDirection(tr)))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish))
                .OnExit(() => ResetViewModel(UIViewType.PRCreation));

            uiStateMachine.Configure(UIViewType.Login)
                .OnEntry(tr => RunView(UIViewType.Login, CalculateDirection(tr)))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.TwoFactor)
                .OnEntry(tr => RunView(UIViewType.TwoFactor, CalculateDirection(tr)))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.Gist)
                .OnEntry(tr => RunView(UIViewType.Gist, CalculateDirection(tr)))
                .PermitDynamic(Trigger.Next, () => Go(Trigger.Next))
                .PermitDynamic(Trigger.Cancel, () => Go(Trigger.Cancel))
                .PermitDynamic(Trigger.Finish, () => Go(Trigger.Finish));

            uiStateMachine.Configure(UIViewType.LogoutRequired)
                .OnEntry(tr => RunView(UIViewType.LogoutRequired, CalculateDirection(tr)))
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
                    // it's important to have the stopping flag set before we do this
                    if (activeFlow == mainFlow)
                    {
                        completion?.OnNext(Success.Value);
                        completion?.OnCompleted();
                    }

                    DisposeFlow(activeFlow);

                    if (activeFlow == mainFlow)
                    {
                        uiProvider.RemoveService(typeof(IConnection), this);
                        transition.OnCompleted();
                        Reset();
                    }
                    else
                        activeFlow = stopping || LoggedIn ? mainFlow : UIControllerFlow.Authentication;
                })
                .PermitDynamic(Trigger.Next, () =>
                {
                    // sets the state to None for the current flow
                    var state = Go(Trigger.Next);

                    if (activeFlow != mainFlow)
                    {
                        if (stopping)
                        {
                            // triggering the End state again so that it can clear up the main flow and really end
                            state = Go(Trigger.Finish, mainFlow);
                        }
                        else
                        {
                            // sets the state to the first UI of the main flow or the auth flow, depending on whether you're logged in
                            state = Go(Trigger.Next, LoggedIn ? mainFlow : UIControllerFlow.Authentication);
                        }
                    }
                    return state;
                })
                .Permit(Trigger.Finish, UIViewType.None);
        }

        void ResetViewModel(UIViewType viewType)
        {
            var flowList = GetObjectsForFlow(activeFlow);
            IUIPair pair;
            if (flowList.TryGetValue(viewType, out pair))
            {
                pair.ViewModel.Reset();
            }
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
                // allows jumping to a specific view
                .PermitDynamic(Trigger.Next, () => 
                    requestedTarget == null || requestedTarget.ViewType == UIViewType.None
                        ? UIViewType.PRList
                        : requestedTarget.ViewType)
                .Permit(Trigger.Finish, UIViewType.End);

            logic.Configure(UIViewType.PRList)
                // allows jumping to a specific view or reload the current view
                .PermitDynamic(Trigger.Next, () =>
                    requestedTarget == null || requestedTarget.ViewType == UIViewType.None
                        ? UIViewType.PRList
                        : requestedTarget.ViewType)
                .Permit(Trigger.PRDetail, UIViewType.PRDetail)
                .Permit(Trigger.PRCreation, UIViewType.PRCreation)
                .Permit(Trigger.Cancel, UIViewType.End)
                .Permit(Trigger.Finish, UIViewType.End);

            logic.Configure(UIViewType.PRDetail)
                // allows jumping to a specific view or reload the current view
                .PermitDynamic(Trigger.Next, () =>
                    requestedTarget == null
                        ? UIViewType.PRList
                        : requestedTarget.ViewType == UIViewType.None
                            ? UIViewType.PRDetail 
                            : requestedTarget.ViewType)
                .Permit(Trigger.Cancel, UIViewType.PRList)
                .Permit(Trigger.Finish, UIViewType.End);

            logic.Configure(UIViewType.PRCreation)
                // allows jumping to a specific view or reload the current view
                .PermitDynamic(Trigger.Next, () =>
                    requestedTarget == null
                        ? UIViewType.PRList
                        : requestedTarget.ViewType == UIViewType.None
                            ? UIViewType.PRDetail
                            : requestedTarget.ViewType)
                .Permit(Trigger.Cancel, UIViewType.PRList)
                .Permit(Trigger.Finish, UIViewType.End);

            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(UIControllerFlow.PullRequests, logic);

            // gist flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.Gist)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.Gist)
                .Permit(Trigger.Next, UIViewType.End)
                .Permit(Trigger.Cancel, UIViewType.End)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(UIControllerFlow.Gist, logic);

            // logout required flow
            logic = new StateMachine<UIViewType, Trigger>(UIViewType.None);
            logic.Configure(UIViewType.None)
                .Permit(Trigger.Next, UIViewType.LogoutRequired)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.LogoutRequired)
                .Permit(Trigger.Next, UIViewType.End)
                .Permit(Trigger.Cancel, UIViewType.End)
                .Permit(Trigger.Finish, UIViewType.End);
            logic.Configure(UIViewType.End)
                .Permit(Trigger.Next, UIViewType.None);
            machines.Add(UIControllerFlow.LogoutRequired, logic);
        }

        UIControllerFlow SelectActiveFlow()
        {
            var host = connection != null ? hosts.LookupHost(connection.HostAddress) : null;
            var loggedIn = host?.IsLoggedIn ?? false;
            if (!loggedIn || mainFlow != UIControllerFlow.Gist)
                return loggedIn ? mainFlow : UIControllerFlow.Authentication;

            var supportsGist = host?.SupportsGist ?? false;
            return supportsGist ? mainFlow : UIControllerFlow.LogoutRequired;
        }

        static LoadDirection CalculateDirection(StateMachineType.Transition tr)
        {
            if (tr.IsReentry)
                return LoadDirection.None;
            switch (tr.Trigger)
            {
                case Trigger.Cancel:
                    return LoadDirection.Back;
                case Trigger.None:
                    return LoadDirection.None;
                default:
                    return LoadDirection.Forward;
            }
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
            if (activeFlow == mainFlow || !Success.Value)
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

        void RunView(UIViewType viewType, LoadDirection direction, ViewWithData arg = null)
        {
            if (requestedTarget?.ViewType == viewType)
            {
                arg = requestedTarget;
                requestedTarget = null;
            }

            if (arg == null)
                arg = new ViewWithData { ActiveFlow = activeFlow, MainFlow = mainFlow, ViewType = viewType };
            bool firstTime = CreateViewAndViewModel(viewType, arg);
            var view = GetObjectsForFlow(activeFlow)[viewType].View;
            transition.OnNext(new LoadData
            {
                View = view,
                Data = arg,
                Direction = direction
            });

            // controller might have been stopped in the OnNext above
            if (IsStopped)
                return;

            // if it's not the first time we've shown this view, no need
            // to set it up
            if (!firstTime)
                return;

            SetupView(viewType, view);
        }

        void SetupView(UIViewType viewType, IView view)
        {
            var list = GetObjectsForFlow(activeFlow);
            var pair = list[viewType];

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
                pair2fa.AddHandler(pair2fa.ViewModel.WhenAny(x => x.IsShowing, x => x.Value)
                    .Where(x => x)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Next)));

                pair2fa.AddHandler(pair2fa.View.Cancel
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Cancel) ? Trigger.Cancel : Trigger.Finish)));

                pair.AddHandler(view.Done
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(Trigger.Finish)));
            }
            else
            {
                pair.AddHandler(view.Done
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Next) ? Trigger.Next : Trigger.Finish)));

                var cv = view as IHasCreationView;
                if (cv != null)
                    pair.AddHandler(cv.Create
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ => Fire(Trigger.PRCreation)));

                var dv = view as IHasDetailView;
                if (dv != null)
                    pair.AddHandler(dv.Open
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => Fire(Trigger.PRDetail, x)));
            }

            pair.AddHandler(view.Cancel
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Fire(uiStateMachine.CanFire(Trigger.Cancel) ? Trigger.Cancel : Trigger.Finish)));
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
            if (arg != null && triggers.ContainsKey(next))
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
        public UIControllerFlow SelectedFlow => mainFlow;
        bool LoggedIn => connection != null && hosts.LookupHost(connection.HostAddress).IsLoggedIn;
        bool? Success { get; set; }
    }
}
