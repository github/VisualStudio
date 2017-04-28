The `UIController` class creates views and associated viewmodels for each UI we have,
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