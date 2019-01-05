// (c) Copyright Microsoft Corporation.
// (c) Copyright GitHub, Inc.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI.Controls;
using GitHub.UI.Controls.AutoCompleteBox;
using GitHub.UI.Helpers;
using ReactiveUI;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace GitHub.UI
{
    /// <summary>
    /// Represents a control that provides a text box for user input and a
    /// drop-down that contains possible matches based on the input in the text
    /// box.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "It's a control. It'll be disposed when the app shuts down.")]
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
        Justification = "Large implementation keeps the components contained.")]
    [ContentProperty("ItemsSource")]
    public class AutoCompleteBox : Control, IUpdateVisualState, IPopupTarget
    {
        private const string elementSelector = "Selector";
        private const string elementPopup = "Popup";
        private const string elementTextBoxStyle = "TextBoxStyle";
        private const string elementItemContainerStyle = "ItemContainerStyle";

        private readonly IDictionary<string, IDisposable> eventSubscriptions = new Dictionary<string, IDisposable>();
        private List<object> suggestions; // local cached copy of the items data.

        /// <summary>
        // Gets or sets the observable collection that contains references to 
        // all of the items in the generated view of data that is provided to 
        /// the selection-style control adapter.
        /// </summary>
        private ObservableCollection<object> view;

        /// <summary>
        /// Gets or sets a value to ignore a number of pending change handlers. 
        /// The value is decremented after each use. This is used to reset the 
        /// value of properties without performing any of the actions in their 
        /// change handlers.
        /// </summary>
        /// <remarks>The int is important as a value because the TextBox 
        /// TextChanged event does not immediately fire, and this will allow for
        /// nested property changes to be ignored.</remarks>
        private int ignoreTextPropertyChange;
        private bool ignorePropertyChange; // indicates whether to ignore calling pending change handlers.
        private bool userCalledPopulate; // indicates whether the user initiated the current populate call.
        private bool popupHasOpened; // A value indicating whether the popup has been opened at least once.
        // Helper that provides all of the standard interaction functionality. Making it internal for subclass access.
        internal InteractionHelper Interaction { get; set; }
        // BindingEvaluator that provides updated string values from a single binding.
        /// A weak event listener for the collection changed event.
        private WeakEventListener<AutoCompleteBox, object, NotifyCollectionChangedEventArgs> collectionChangedWeakEventListener;
        bool supportsShortcutOriginalValue; // Used to save whether the text input allows shortcuts or not.
        readonly Subject<Unit> populatingSubject = new Subject<Unit>();
        readonly IDpiManager dpiManager;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> class.
        /// </summary>
        public AutoCompleteBox() : this(DpiManager.Instance)
        {
        }

        public AutoCompleteBox(IDpiManager dpiManager)
        {
            Guard.ArgumentNotNull(dpiManager, "dpiManager");

            CompletionOffset = 0;
            IsEnabledChanged += ControlIsEnabledChanged;
            Interaction = new InteractionHelper(this);

            // Creating the view here ensures that View is always != null
            ClearView();

            Populating = populatingSubject;

            Populating
                .SelectMany(_ =>
                {
                    var advisor = Advisor ?? EmptyAutoCompleteAdvisor.Instance;
                    return advisor.GetAutoCompletionSuggestions(Text, TextBox.CaretIndex);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(result =>
                {
                    CompletionOffset = result.Offset;
                    ItemsSource = result.Suggestions;
                    PopulateComplete();
                });
            this.dpiManager = dpiManager;
        }

        public IObservable<Unit> Populating { get; private set; }

        public int CompletionOffset
        {
            get { return (int)GetValue(CompletionOffsetProperty); }
            set { SetValue(CompletionOffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CompletionOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompletionOffsetProperty =
            DependencyProperty.Register(
                "CompletionOffset",
                typeof(int),
                typeof(AutoCompleteBox),
                new PropertyMetadata(0));

        public Point PopupPosition
        {
            get
            {
                var position = TextBox.GetPositionFromCharIndex(CompletionOffset);
                var dpi = dpiManager.CurrentDpi;
                double verticalOffset = 5.0 - TextBox.Margin.Bottom;
                position.Offset(0, verticalOffset);  // Vertically pad it. Yeah, Point is mutable. WTF?
                return dpi.Scale(position);
            }
        }

        /// <summary>
        /// Gets or sets the minimum delay, in milliseconds, after text is typed
        /// in the text box before the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control
        /// populates the list of possible matches in the drop-down.
        /// </summary>
        /// <value>The minimum delay, in milliseconds, after text is typed in
        /// the text box, but before the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> populates
        /// the list of possible matches in the drop-down. The default is 0.</value>
        /// <exception cref="T:System.ArgumentException">The set value is less than 0.</exception>
        public int MinimumPopulateDelay
        {
            get { return (int)GetValue(MinimumPopulateDelayProperty); }
            set { SetValue(MinimumPopulateDelayProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="MinimumPopulateDelay" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="MinimumPopulateDelay" />
        /// dependency property.</value>
        public static readonly DependencyProperty MinimumPopulateDelayProperty =
            DependencyProperty.Register(
                "MinimumPopulateDelay",
                typeof(int),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnMinimumPopulateDelayPropertyChanged));

        /// <summary>
        /// MinimumPopulateDelayProperty property changed handler. Any current 
        /// dispatcher timer will be stopped. The timer will not be restarted 
        /// until the next TextUpdate call by the user.
        /// </summary>
        /// <param name="d">AutoCompleteTextBox that changed its 
        /// MinimumPopulateDelay.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly",
            Justification = "The exception is most likely to be called through the CLR property setter.")]
        private static void OnMinimumPopulateDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;

            if (source == null) return;

            if (source.ignorePropertyChange)
            {
                source.ignorePropertyChange = false;
                return;
            }

            int newValue = (int)e.NewValue;
            if (newValue < 0)
            {
                source.ignorePropertyChange = true;
                d.SetValue(e.Property, e.OldValue);

                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "Invalid value '{0}' for MinimumPopulateDelay", newValue), "e");
            }

            // Resubscribe to TextBox changes with new delay. The easiest way is to just set the TextBox to itself.
            var textBox = source.TextBox;
            source.TextBox = null;
            source.TextBox = textBox;
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate" /> used
        /// to display each item in the drop-down portion of the control.
        /// </summary>
        /// <value>The <see cref="DataTemplate" /> used to
        /// display each item in the drop-down. The default is null.</value>
        /// <remarks>
        /// You use the ItemTemplate property to specify the visualization 
        /// of the data objects in the drop-down portion of the AutoCompleteBox 
        /// control. If your AutoCompleteBox is bound to a collection and you 
        /// do not provide specific display instructions by using a 
        /// DataTemplate, the resulting UI of each item is a string 
        /// representation of each object in the underlying collection. 
        /// </remarks>
        public DataTemplate ItemTemplate
        {
            get { return GetValue(ItemTemplateProperty) as DataTemplate; }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="AutoCompleteBox.ItemTemplate" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="AutoCompleteBox.ItemTemplate" />
        /// dependency property.</value>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                "ItemTemplate",
                typeof(DataTemplate),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Style" /> that is
        /// applied to the selection adapter contained in the drop-down portion
        /// of the <see cref="GitHub.UI.AutoCompleteBox" />
        /// control.
        /// </summary>
        /// <value>The <see cref="T:System.Windows.Style" /> applied to the
        /// selection adapter contained in the drop-down portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.
        /// The default is null.</value>
        /// <remarks>
        /// The default selection adapter contained in the drop-down is a 
        /// ListBox control. 
        /// </remarks>
        public Style ItemContainerStyle
        {
            get { return GetValue(ItemContainerStyleProperty) as Style; }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="AutoCompleteBox.ItemContainerStyle" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="AutoCompleteBox.ItemContainerStyle" />
        /// dependency property.</value>
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(
                elementItemContainerStyle,
                typeof(Style),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null, null));

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Style" /> applied to
        /// the text box portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The <see cref="T:System.Windows.Style" /> applied to the text
        /// box portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.
        /// The default is null.</value>
        public Style TextBoxStyle
        {
            get { return GetValue(TextBoxStyleProperty) as Style; }
            set { SetValue(TextBoxStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="AutoCompleteBox.TextBoxStyle" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="AutoCompleteBox.TextBoxStyle" />
        /// dependency property.</value>
        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register(
                elementTextBoxStyle,
                typeof(Style),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="AutoCompleteBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="AutoCompleteBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(
                "MaxDropDownHeight",
                typeof(double),
                typeof(AutoCompleteBox),
                new PropertyMetadata(double.PositiveInfinity, OnMaxDropDownHeightPropertyChanged));

        /// <summary>
        /// MaxDropDownHeightProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteTextBox that changed its MaxDropDownHeight.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly"
            , Justification = "The exception will be called through a CLR setter in most cases.")]
        private static void OnMaxDropDownHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            if (source == null) return;
            if (source.ignorePropertyChange)
            {
                source.ignorePropertyChange = false;
                return;
            }

            double newValue = (double)e.NewValue;
            
            // Revert to the old value if invalid (negative)
            if (newValue < 0)
            {
                source.ignorePropertyChange = true;
                source.SetValue(e.Property, e.OldValue);

                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "Invalid value '{0}' for MaxDropDownHeight", e.NewValue), "e");
            }

            source.OnMaxDropDownHeightChanged(newValue);
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the drop-down portion of
        /// the control is open.
        /// </summary>
        /// <value>
        /// True if the drop-down is open; otherwise, false. The default is
        /// false.
        /// </value>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set
            {
                HandleShortcutSupport(value);
                SetValue(IsDropDownOpenProperty, value);
            }
        }

        void HandleShortcutSupport(bool value)
        {
            if (TextBox == null)
            {
                return;
            }

            var shortcutContainer = TextBox.Control as IShortcutContainer;
            if (shortcutContainer != null)
            {
                shortcutContainer.SupportsKeyboardShortcuts = !value && supportsShortcutOriginalValue;
            }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="AutoCompleteBox.IsDropDownOpen" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="AutoCompleteBox.IsDropDownOpen" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(
                "IsDropDownOpen",
                typeof(bool),
                typeof(AutoCompleteBox),
                new PropertyMetadata(false, OnIsDropDownOpenPropertyChanged));

        /// <summary>
        /// IsDropDownOpenProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteTextBox that changed its IsDropDownOpen.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsDropDownOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;

            if (source == null) return;

            // Ignore the change if requested
            if (source.ignorePropertyChange)
            {
                source.ignorePropertyChange = false;
                return;
            }

            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;

            if (!newValue)
            {
                source.ClosingDropDown(oldValue);
            }

            source.UpdateVisualState(true);
        }

        /// <summary>
        /// Gets or sets a collection that is used to generate the items for the
        /// drop-down portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The collection that is used to generate the items of the
        /// drop-down portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.</value>
        public IEnumerable ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as IEnumerable; }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="AutoCompleteBox.ItemsSource" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="AutoCompleteBox.ItemsSource" />
        /// dependency property.</value>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnItemsSourcePropertyChanged));

        /// <summary>
        /// ItemsSourceProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its ItemsSource.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var autoComplete = d as AutoCompleteBox;
            if (autoComplete == null) return;
            autoComplete.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the selected item in the drop-down.
        /// </summary>
        /// <value>The selected item in the drop-down.</value>
        /// <remarks>
        /// If the IsTextCompletionEnabled property is true and text typed by 
        /// the user matches an item in the ItemsSource collection, which is 
        /// then displayed in the text box, the SelectedItem property will be 
        /// a null reference.
        /// </remarks>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="AutoCompleteBox.SelectedItem" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier the
        /// <see cref="AutoCompleteBox.SelectedItem" />
        /// dependency property.</value>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(AutoCompleteBox),
                new PropertyMetadata());

        private void CancelSuggestion()
        {
            Debug.Assert(TextBox != null, "TextBox is somehow null");
            Debug.Assert(Text != null, "Text is somehow null");

            DismissDropDown();

            Debug.Assert(0 == TextBox.SelectionLength, "SelectionLength is what I think it is");
        }

        private void ExpandSuggestion(string value)
        {
            Debug.Assert(value != null, "The string passed into ExpandSuggestion should not be null");
            Debug.Assert(TextBox != null, "TextBox is somehow null");
            Debug.Assert(Text != null, "Text is somehow null");

            var newText = TextBox.GetExpandedText(value, CompletionOffset);
            UpdateTextValue(newText);

            // New caret index should be one space after the inserted text.
            int newCaretIndex = CompletionOffset + value.Length + 1;
            TextBox.CaretIndex = newCaretIndex;
            Debug.Assert(newCaretIndex == TextBox.SelectionStart,
                String.Format(CultureInfo.InvariantCulture,
                "SelectionStart '{0}' should be the same as newCaretIndex '{1}'",
                TextBox.SelectionStart, newCaretIndex));
            Debug.Assert(0 == TextBox.SelectionLength,
                String.Format(CultureInfo.InvariantCulture,
                "SelectionLength should be 0 but is '{0}' is what I think it is",
                TextBox.SelectionStart));
        }

        /// <summary>
        /// Gets or sets the text in the text box portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The text in the text box portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control.</value>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="AutoCompleteBox.Text" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="AutoCompleteBox.Text" />
        /// dependency property.</value>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(AutoCompleteBox),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        /// <summary>
        /// TextProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its Text.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            if (source == null) return;

            source.OnTextPropertyChanged((string) e.NewValue);
        }

        /// <summary>
        /// Gets or sets the drop down popup control.
        /// </summary>
        private PopupHelper DropDownPopup { get; set; }

        /// <summary>
        /// The TextBox template part.
        /// </summary>
        private IAutoCompleteTextInput textInput;

        /// <summary>
        /// The SelectionAdapter.
        /// </summary>
        private ISelectionAdapter adapter;

        /// <summary>
        /// Gets or sets the Text template part.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IAutoCompleteTextInput TextBox
        {
            get { return textInput; }
            set { UpdateTextBox(value); }
        }

        void UpdateTextBox(IAutoCompleteTextInput value)
        {
            // Detach existing handlers
            if (textInput != null)
            {
                UnsubscribeToEvent("SelectionChanged");
                UnsubscribeToEvent("OnTextBoxTextChanged");
            }

            textInput = value;

            // Attach handlers
            if (textInput != null)
            {
                var shortcutContainer = textInput.Control as IShortcutContainer;
                if (shortcutContainer != null)
                {
                    supportsShortcutOriginalValue = shortcutContainer.SupportsKeyboardShortcuts;
                }

                SubscribeToEvent("OnTextBoxTextChanged", 
                    ObserveTextBoxChanges().Subscribe(shouldPopulate =>
                    {
                        if (shouldPopulate)
                        {
                            PopulateDropDown();
                        }
                        else
                        {
                            DismissDropDown();
                        }
                    }));

                if (Text != null)
                {
                    UpdateTextValue(Text);
                }
            }
        }

        IObservable<bool> ObserveTextBoxChanges()
        {
            var distinctTextChanges = textInput
                .TextChanged
                .Select(_ => textInput.Text ?? "")
                .DistinctUntilChanged();

            if (MinimumPopulateDelay >= 0)
            {
                distinctTextChanges = distinctTextChanges
                    .Throttle(TimeSpan.FromMilliseconds(MinimumPopulateDelay), RxApp.MainThreadScheduler);
            }

            return distinctTextChanges
                .Select(text => {
                    bool userChangedTextBox = ignoreTextPropertyChange == 0;
                    if (ignoreTextPropertyChange > 0) ignoreTextPropertyChange--;

                    return new { Text = text, ShouldPopulate = text.Length > 0 && userChangedTextBox };
                })
                .Do(textInfo =>
                {
                    userCalledPopulate = textInfo.ShouldPopulate;
                    UpdateAutoCompleteTextValue(textInfo.Text);
                })
                .Select(textInfo => textInfo.ShouldPopulate);
        }

        /// <summary>
        /// Gets or sets the selection adapter used to populate the drop-down
        /// with a list of selectable items.
        /// </summary>
        /// <value>The selection adapter used to populate the drop-down with a
        /// list of selectable items.</value>
        /// <remarks>
        /// You can use this property when you create an automation peer to sw
        /// use with AutoCompleteBox or deriving from AutoCompleteBox to 
        /// create a custom control.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ISelectionAdapter SelectionAdapter
        {
            get { return adapter; }
            set
            {
                if (adapter != null)
                {
                    adapter.SelectionChanged -= OnAdapterSelectionChanged;
                    adapter.Commit -= OnAdapterSelectionComplete;
                    adapter.Cancel -= OnAdapterSelectionCanceled;
                    adapter.ItemsSource = null;
                }

                adapter = value;

                if (adapter != null)
                {
                    adapter.SelectionChanged += OnAdapterSelectionChanged;
                    adapter.Commit += OnAdapterSelectionComplete;
                    adapter.Cancel += OnAdapterSelectionCanceled;
                    adapter.ItemsSource = view;
                }
            }
        }

        /// <summary>
        /// Provides suggestions based on what's been typed.
        /// </summary>
        public IAutoCompleteAdvisor Advisor
        {
            get;
            set;
        }

        /// <summary>
        /// Identifies the
        /// <see cref="IAutoCompleteAdvisor" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="IAutoCompleteAdvisor" />
        /// dependency property.</value>
        public static readonly DependencyProperty AdvisorProperty =
            DependencyProperty.Register(
                "Advisor",
                typeof(IAutoCompleteAdvisor),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null, OnAdvisorPropertyChanged));

        /// <summary>
        /// AdvisorProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its Advisor.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnAdvisorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            if (source == null) return;

            source.Advisor = (IAutoCompleteAdvisor)e.NewValue;
        }

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (TextBox != null)
            {
                UnsubscribeToEvent("PreviewKeyDown");
            }

            if (DropDownPopup != null)
            {
                DropDownPopup.Closed -= OnDropDownClosed;
                DropDownPopup.FocusChanged -= OnDropDownFocusChanged;
                DropDownPopup.UpdateVisualStates -= OnDropDownPopupUpdateVisualStates;
                DropDownPopup.BeforeOnApplyTemplate();
                DropDownPopup = null;
            }

            base.OnApplyTemplate();

            // Set the template parts. Individual part setters remove and add 
            // any event handlers.
            var popup = GetTemplateChild(elementPopup) as Popup;
            if (popup != null)
            {
                DropDownPopup = new PopupHelper(this, popup)
                {
                    MaxDropDownHeight = MaxDropDownHeight
                };
                DropDownPopup.AfterOnApplyTemplate();
                DropDownPopup.Closed += OnDropDownClosed;
                DropDownPopup.FocusChanged += OnDropDownFocusChanged;
                DropDownPopup.UpdateVisualStates += OnDropDownPopupUpdateVisualStates;
            }
            SelectionAdapter = GetSelectionAdapterPart();
            // TODO: eliminate duplication between these two elements...
            TextBox = InputElement;

            if (TextBox != null)
            {
                SubscribeToEvent("PreviewKeyDown", TextBox.PreviewKeyDown.Subscribe(OnTextBoxPreviewKeyDown));
            }

            Interaction.OnApplyTemplateBase();

            // If the drop down property indicates that the popup is open,
            // flip its value to invoke the changed handler.
            if (IsDropDownOpen && DropDownPopup != null && !DropDownPopup.IsOpen)
            {
                OpeningDropDown();
            }
        }

        /// <summary>
        /// Allows the popup wrapper to fire visual state change events.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnDropDownPopupUpdateVisualStates(object sender, EventArgs e)
        {
            UpdateVisualState(true);
        }

        /// <summary>
        /// Allows the popup wrapper to fire the FocusChanged event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnDropDownFocusChanged(object sender, EventArgs e)
        {
            FocusChanged(HasFocus());
        }

        /// <summary>
        /// Begin closing the drop-down.
        /// </summary>
        /// <param name="oldValue">The original value.</param>
        private void ClosingDropDown(bool oldValue)
        {
            bool delayedClosingVisual = false;
            if (DropDownPopup != null)
            {
                delayedClosingVisual = DropDownPopup.UsesClosingVisualState;
            }

            if (view == null || view.Count == 0)
            {
                delayedClosingVisual = false;
            }

            // Immediately close the drop down window:
            // When a popup closed visual state is present, the code path is 
            // slightly different and the actual call to CloseDropDown will 
            // be called only after the visual state's transition is done
            RaiseExpandCollapseAutomationEvent(oldValue, false);
            if (!delayedClosingVisual)
            {
                CloseDropDown();
            }

            UpdateVisualState(true);
        }

        private void OpeningDropDown()
        {
            OpenDropDown();

            UpdateVisualState(true);
        }

        /// <summary>
        /// Raise an expand/collapse event through the automation peer.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
        {
            var peer = UIElementAutomationPeer.FromElement(this) as AutoCompleteBoxAutomationPeer;
            if (peer != null)
            {
                peer.RaiseExpandCollapseAutomationEvent(oldValue, newValue);
            }
        }

        /// <summary>
        /// Handles the PreviewKeyDown event on the TextBox for WPF.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void OnTextBoxPreviewKeyDown(EventPattern<KeyEventArgs> e)
        {
            OnKeyDown(e.EventArgs);
        }

        /// <summary>
        /// Connects to the DropDownPopup Closed event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnDropDownClosed(object sender, EventArgs e)
        {
            // Force the drop down dependency property to be false.
            if (IsDropDownOpen)
            {
                IsDropDownOpen = false;
            }
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:GitHub.UI.AutoCompleteBoxAutomationPeer" />
        /// </summary>
        /// <returns>A
        /// <see cref="T:GitHub.UI.AutoCompleteBoxAutomationPeer" />
        /// for the <see cref="GitHub.UI.AutoCompleteBox" />
        /// object.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AutoCompleteBoxAutomationPeer(this);
        }

        /// <summary>
        /// Handles the FocusChanged event.
        /// </summary>
        /// <param name="hasFocus">A value indicating whether the control 
        /// currently has the focus.</param>
        private void FocusChanged(bool hasFocus)
        {
            // The OnGotFocus & OnLostFocus are asynchronously and cannot 
            // reliably tell you that have the focus.  All they do is let you 
            // know that the focus changed sometime in the past.  To determine 
            // if you currently have the focus you need to do consult the 
            // FocusManager (see HasFocus()).

            if (!hasFocus)
            {
                IsDropDownOpen = false;
                userCalledPopulate = false;
            }
        }

        /// <summary>
        /// Determines whether the text box or drop-down portion of the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> control has
        /// focus.
        /// </summary>
        /// <returns>true to indicate the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> has focus;
        /// otherwise, false.</returns>
        protected bool HasFocus()
        {
            var focused =
                // For WPF, check if the element that has focus is within the control, as
                // FocusManager.GetFocusedElement(this) will return null in such a case.
                IsKeyboardFocusWithin ? Keyboard.FocusedElement as DependencyObject : FocusManager.GetFocusedElement(this) as DependencyObject;

            while (focused != null)
            {
                if (ReferenceEquals(focused, this))
                {
                    return true;
                }

                // This helps deal with popups that may not be in the same 
                // visual tree
                var parent = VisualTreeHelper.GetParent(focused);
                if (parent == null)
                {
                    // Try the logical parent.
                    var element = focused as FrameworkElement;
                    if (element != null)
                    {
                        parent = element.Parent;
                    }
                }
                focused = parent;
            }
            return false;
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.GotFocus" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" />
        /// that contains the event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            FocusChanged(HasFocus());
        }

        /// <summary>
        /// Handles change of keyboard focus, which is treated differently than control focus
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            FocusChanged((bool)e.NewValue);
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.LostFocus" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" />
        /// that contains the event data.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            FocusChanged(HasFocus());
        }

        /// <summary>
        /// Handle the change of the IsEnabled property.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void ControlIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isEnabled = (bool)e.NewValue;
            if (!isEnabled)
            {
                IsDropDownOpen = false;
            }
        }

        /// <summary>
        /// Returns the
        /// <see cref="T:Chat.ISelectionAdapter" /> part, if
        /// possible.
        /// </summary>
        /// <returns>
        /// A <see cref="T:Chat.ISelectionAdapter" /> object,
        /// if possible. Otherwise, null.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Following the GetTemplateChild pattern for the method.")]
        protected virtual ISelectionAdapter GetSelectionAdapterPart()
        {
            var selector = GetTemplateChild(elementSelector) as Selector;
            if (selector != null)
            {
                // Built in support for wrapping a Selector control
                adapter = new SelectorSelectionAdapter(selector);
            }
            return adapter;
        }

        /// <summary>
        /// Populates the drop down
        /// </summary>
        private void PopulateDropDown()
        {
            populatingSubject.OnNext(Unit.Default);
        }

        void DismissDropDown()
        {
            SelectedItem = null;
            if (IsDropDownOpen)
            {
                IsDropDownOpen = false;
            }
        }

        /// <summary>
        /// Converts the specified object to a string by using the
        /// <see cref="System.Windows.Data.Binding.Converter" /> and
        /// <see cref="System.Windows.Data.Binding.ConverterCulture" /> values
        /// of the binding object specified by the
        /// <see cref="AutoCompleteBox.ValueMemberBinding" />
        /// property.
        /// </summary>
        /// <param name="value">The object to format as a string.</param>
        /// <returns>The string representation of the specified object.</returns>
        /// <remarks>
        /// Override this method to provide a custom string conversion.
        /// </remarks>
        protected virtual string FormatValue(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        /// <summary>
        /// Updates both the text box value and underlying text dependency 
        /// property value if and when they change. Automatically fires the 
        /// text changed events when there is a change.
        /// </summary>
        /// <param name="value">The new string value.</param>
        private void UpdateTextValue(string value)
        {
            UpdateAutoCompleteTextValue(value);
            UpdateTextBoxValue(value);
        }

        // Update the TextBox's Text dependency property
        void UpdateTextBoxValue(string value)
        {
            var newValue = value ?? string.Empty;

            if (TextBox == null || TextBox.Text == newValue)
            {
                return;
            }

            ignoreTextPropertyChange++;
            TextBox.Text = newValue;
        }

        void UpdateAutoCompleteTextValue(string value)
        {
            if (Text == value) return;

            ignoreTextPropertyChange++;
            Text = value;
        }

        /// <summary>
        /// Handle the update of the text when the Text dependency property changes.
        /// </summary>
        /// <param name="newText">The new text.</param>
        private void OnTextPropertyChanged(string newText)
        {
            // Only process this event if it is coming from someone outside 
            // setting the Text dependency property directly.
            if (ignoreTextPropertyChange > 0)
            {
                ignoreTextPropertyChange--;
                return;
            }

            UpdateTextBoxValue(newText);
        }

        /// <summary>
        /// Notifies the
        /// <see cref="GitHub.UI.AutoCompleteBox" /> that the
        /// <see cref="AutoCompleteBox.ItemsSource" />
        /// property has been set and the data can be filtered to provide
        /// possible matches in the drop-down.
        /// </summary>
        /// <remarks>
        /// Call this method when you are providing custom population of 
        /// the drop-down portion of the AutoCompleteBox, to signal the control 
        /// that you are done with the population process. 
        /// Typically, you use PopulateComplete when the population process 
        /// is a long-running process and you want to cancel built-in filtering
        ///  of the ItemsSource items. In this case, you can handle the 
        /// Populated event and set PopulatingEventArgs.Cancel to true. 
        /// When the long-running process has completed you call 
        /// PopulateComplete to indicate the drop-down is populated.
        /// </remarks>
        protected void PopulateComplete()
        {
            RefreshView();

            if (SelectionAdapter != null && !Equals(SelectionAdapter.ItemsSource, view))
            {
                SelectionAdapter.ItemsSource = view;
            }

            bool isDropDownOpen = userCalledPopulate && (view.Count > 0);
            if (isDropDownOpen != IsDropDownOpen)
            {
                ignorePropertyChange = true;
                IsDropDownOpen = isDropDownOpen;
            }
            if (IsDropDownOpen)
            {
                OpeningDropDown();
            }
            else
            {
                ClosingDropDown(true);
            }

            // We always want to select the first suggestion after populating the drop down.
            SelectFirstItem();
        }

        void SelectFirstItem()
        {
            if (!view.Any()) return;

            var newSelectedItem = view.First();
            SelectionAdapter.SelectedItem = newSelectedItem;
            SelectedItem = newSelectedItem;
        }


        /// <summary>
        /// A simple helper method to clear the view and ensure that a view 
        /// object is always present and not null.
        /// </summary>
        private void ClearView()
        {
            if (view == null)
            {
                view = new ObservableCollection<object>();
            }
            else
            {
                view.Clear();
            }
        }

        /// <summary>
        /// Walks through the items enumeration. Performance is not going to be perfect with the current implementation.
        /// </summary>
        private void RefreshView()
        {
            if (suggestions == null)
            {
                ClearView();
                return;
            }

            int viewIndex = 0;
            int viewCount = view.Count;
            var items = suggestions;
            foreach (var item in items)
            {
                if (viewCount > viewIndex && view[viewIndex] == item)
                {
                    // Item is still in the view
                    viewIndex++;
                }
                else
                {
                    // Insert the item
                    if (viewCount > viewIndex && view[viewIndex] != item)
                    {
                        // Replace item
                        // Unfortunately replacing via index throws a fatal 
                        // exception: View[view_index] = item;
                        // Cost: O(n) vs O(1)
                        view.RemoveAt(viewIndex);
                        view.Insert(viewIndex, item);
                        viewIndex++;
                    }
                    else
                    {
                        // Add the item
                        if (viewIndex == viewCount)
                        {
                            // Constant time is preferred (Add).
                            view.Add(item);
                        }
                        else
                        {
                            view.Insert(viewIndex, item);
                        }
                        viewIndex++;
                        viewCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Handle any change to the ItemsSource dependency property, update 
        /// the underlying ObservableCollection view, and set the selection 
        /// adapter's ItemsSource to the view if appropriate.
        /// </summary>
        /// <param name="oldValue">The old enumerable reference.</param>
        /// <param name="newValue">The new enumerable reference.</param>
        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove handler for oldValue.CollectionChanged (if present)
            var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;
            if (null != oldValueINotifyCollectionChanged && null != collectionChangedWeakEventListener)
            {
                collectionChangedWeakEventListener.Detach();
                collectionChangedWeakEventListener = null;
            }

            // Add handler for newValue.CollectionChanged (if possible)
            var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                collectionChangedWeakEventListener = new WeakEventListener<AutoCompleteBox, object, NotifyCollectionChangedEventArgs>(this)
                {
                    OnEventAction =
                        (instance, source, eventArgs) => instance.ItemsSourceCollectionChanged(eventArgs),
                    OnDetachAction =
                        weakEventListener =>
                        newValueINotifyCollectionChanged.CollectionChanged -= weakEventListener.OnEvent
                };
                newValueINotifyCollectionChanged.CollectionChanged += collectionChangedWeakEventListener.OnEvent;
            }

            // Store a local cached copy of the data
            suggestions = newValue == null ? null : new List<object>(newValue.Cast<object>().ToList());

            // Clear and set the view on the selection adapter
            ClearView();
            if (SelectionAdapter != null && !Equals(SelectionAdapter.ItemsSource, view))
            {
                SelectionAdapter.ItemsSource = view;
            }
            if (IsDropDownOpen)
            {
                RefreshView();
            }
        }

        /// <summary>
        /// Method that handles the ObservableCollection.CollectionChanged event for the ItemsSource property.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void ItemsSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Update the cache
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                for (int index = 0; index < e.OldItems.Count; index++)
                {
                    suggestions.RemoveAt(e.OldStartingIndex);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && suggestions.Count >= e.NewStartingIndex)
            {
                for (int index = 0; index < e.NewItems.Count; index++)
                {
                    suggestions.Insert(e.NewStartingIndex + index, e.NewItems[index]);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Replace && e.NewItems != null && e.OldItems != null)
            {
                foreach (var t in e.NewItems)
                {
                    suggestions[e.NewStartingIndex] = t;
                }
            }

            // Update the view
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (var t in e.OldItems)
                    {
                        view.Remove(t);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // Significant changes to the underlying data.
                ClearView();
                if (ItemsSource != null)
                {
                    suggestions = new List<object>(ItemsSource.Cast<object>().ToList());
                }
            }

            // Refresh the observable collection used in the selection adapter.
            RefreshView();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the selection adapter.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The selection changed event data.</param>
        private void OnAdapterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = adapter.SelectedItem;
        }

        /// <summary>
        /// Handles the Commit event on the selection adapter.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnAdapterSelectionComplete(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;

            var selectedItem = SelectedItem;
            
            // Completion will update the selected value
            ExpandSuggestion(selectedItem == null ? string.Empty : selectedItem.ToString());

            // This forces the textbox to get keyboard focus, in the case where
            // another part of the control may have temporarily received focus.
            if (TextBox != null)
            {
                // Because LOL WPF focus shit, we need to make sure don't lose the caret index when we give this focus.
                int caretIndex = TextBox.CaretIndex;
                TextBox.Focus();
                TextBox.CaretIndex = caretIndex;
            }
            else
            {
                Focus();
            }
        }

        /// <summary>
        /// Handles the Cancel event on the selection adapter.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnAdapterSelectionCanceled(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;

            CancelSuggestion();

            // This forces the textbox to get keyboard focus, in the case where
            // another part of the control may have temporarily received focus.
            if (TextBox != null)
            {
                TextBox.Focus();
            }
            else
            {
                Focus();
            }
        }

        /// <summary>
        /// Handles MaxDropDownHeightChanged by re-arranging and updating the 
        /// popup arrangement.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newValue",
            Justification = "This makes it easy to add validation or other changes in the future.")]
        private void OnMaxDropDownHeightChanged(double newValue)
        {
            if (DropDownPopup != null)
            {
                DropDownPopup.MaxDropDownHeight = newValue;
            }
            UpdateVisualState(true);
        }

        private void OpenDropDown()
        {
            if (DropDownPopup != null)
            {
                DropDownPopup.IsOpen = true;
            }
            popupHasOpened = true;
        }

        private void CloseDropDown()
        {
            if (popupHasOpened)
            {
                if (SelectionAdapter != null)
                {
                    SelectionAdapter.SelectedItem = null;
                }
                if (DropDownPopup != null)
                {
                    DropDownPopup.IsOpen = false;
                }
            }
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.KeyEventArgs" />
        /// that contains the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            base.OnKeyDown(e);

            if (e.Handled || !IsEnabled)
            {
                return;
            }

            // The drop down is open, pass along the key event arguments to the
            // selection adapter. If it isn't handled by the adapter's logic,
            // then we handle some simple navigation scenarios for controlling
            // the drop down.
            if (IsDropDownOpen)
            {
                if (SelectionAdapter != null)
                {
                    SelectionAdapter.HandleKeyDown(e);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                if (e.Key == Key.Escape)
                {
                    OnAdapterSelectionCanceled(this, new RoutedEventArgs());
                    e.Handled = true;
                }
            }

            // Standard drop down navigation
            switch (e.Key)
            {
//                case Key.F4:
//                    IsDropDownOpen = !IsDropDownOpen;
//                    e.Handled = true;
//                    break;

                case Key.Enter:
                    if (IsDropDownOpen && SelectedItem != null)
                    {
                        OnAdapterSelectionComplete(this, new RoutedEventArgs());
                        e.Handled = true;
                    }
                    
                    break;
            }
        }

        /// <summary>
        /// Update the visual state of the control.
        /// </summary>
        /// <param name="useTransitions">
        /// A value indicating whether to automatically generate transitions to
        /// the new state, or instantly transition to the new state.
        /// </param>
        void IUpdateVisualState.UpdateVisualState(bool useTransitions)
        {
            UpdateVisualState(useTransitions);
        }

        /// <summary>
        /// Update the current visual state of the button.
        /// </summary>
        /// <param name="useTransitions">
        /// True to use transitions when updating the visual state, false to
        /// snap directly to the new visual state.
        /// </param>
        internal virtual void UpdateVisualState(bool useTransitions)
        {
            // Popup
            VisualStateManager.GoToState(this, IsDropDownOpen ? VisualStates.StatePopupOpened : VisualStates.StatePopupClosed, useTransitions);

            // Handle the Common and Focused states
            Interaction.UpdateVisualStateBase(useTransitions);
        }

        private class EmptyAutoCompleteAdvisor : IAutoCompleteAdvisor
        {
            public static readonly IAutoCompleteAdvisor Instance = new EmptyAutoCompleteAdvisor();

            private EmptyAutoCompleteAdvisor()
            {
            }

            public IObservable<AutoCompleteResult> GetAutoCompletionSuggestions(string text, int caretPosition)
            {
                return Observable.Empty<AutoCompleteResult>();
            }
        }

        private void SubscribeToEvent(string eventName, IDisposable disposable)
        {
            eventSubscriptions[eventName] = disposable;
        }

        private void UnsubscribeToEvent(string eventName)
        {
            IDisposable disposable;
            if (eventSubscriptions.TryGetValue(eventName, out disposable))
            {
                disposable.Dispose();
            }
        }

        public IAutoCompleteTextInput InputElement
        {
            get { return (IAutoCompleteTextInput)GetValue(InputElementProperty); }
            set { SetValue(InputElementProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="MinimumPopulateDelay" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="MinimumPopulateDelay" />
        /// dependency property.</value>
        public static readonly DependencyProperty InputElementProperty =
            DependencyProperty.Register(
                "InputElement",
                typeof(IAutoCompleteTextInput),
                typeof(AutoCompleteBox));
    }
}
