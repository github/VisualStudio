// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.UI.Controls;

namespace GitHub.UI
{
    /// <summary>
    /// PopupHelper is a simple wrapper type that helps abstract platform
    /// differences out of the Popup.
    /// </summary>
    internal class PopupHelper
    {
        /// <summary>
        /// Gets a value indicating whether a visual popup state is being used
        /// in the current template for the Closed state. Setting this value to
        /// true will delay the actual setting of Popup.IsOpen to false until
        /// after the visual state's transition for Closed is complete.
        /// </summary>
        public bool UsesClosingVisualState { get; private set; }

        /// <summary>
        /// Gets or sets the parent control.
        /// </summary>
        private Control Parent { get; set; }

        /// <summary>
        /// Gets or sets the maximum drop down height value.
        /// </summary>
        public double MaxDropDownHeight { get; set; }

        /// <summary>
        /// Gets the Popup control instance.
        /// </summary>
        public Popup Popup { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the actual Popup is open.
        /// </summary>
        public bool IsOpen
        {
            get { return Popup.IsOpen; }
            set { Popup.IsOpen = value; }
        }

        /// <summary>
        /// Gets or sets the popup child framework element. Can be used if an
        /// assumption is made on the child type.
        /// </summary>
        private FrameworkElement PopupChild { get; set; }

        /// <summary>
        /// The Closed event is fired after the Popup closes.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Fired when the popup children have a focus event change, allows the
        /// parent control to update visual states or react to the focus state.
        /// </summary>
        public event EventHandler FocusChanged;

        /// <summary>
        /// Fired when the popup children intercept an event that may indicate
        /// the need for a visual state update by the parent control.
        /// </summary>
        public event EventHandler UpdateVisualStates;

        /// <summary>
        /// Initializes a new instance of the PopupHelper class.
        /// </summary>
        /// <param name="parent">The parent control.</param>
        public PopupHelper(Control parent)
        {
            Guard.ArgumentNotNull(parent, "parent");
            Parent = parent;
        }

        /// <summary>
        /// Initializes a new instance of the PopupHelper class.
        /// </summary>
        /// <param name="parent">The parent control.</param>
        /// <param name="popup">The Popup template part.</param>
        public PopupHelper(Control parent, Popup popup)
            : this(parent)
        {
            Guard.ArgumentNotNull(parent, "parent");
            Guard.ArgumentNotNull(popup, "popup");
        
            Popup = popup;

            var target = parent as IPopupTarget;
            if (target != null)
            {
                popup.CustomPopupPlacementCallback += (size, targetSize, offset) => new[]
                {
                    new CustomPopupPlacement(target.PopupPosition, PopupPrimaryAxis.Horizontal)
                };
            }
        }

        /// <summary>
        /// Fires the Closed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void OnClosed(EventArgs e)
        {
            var handler = Closed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Actually closes the popup after the VSM state animation completes.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void OnPopupClosedStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            // Delayed closing of the popup until now
            if (e != null && e.NewState != null && e.NewState.Name == VisualStates.StatePopupClosed)
            {
                if (Popup != null)
                {
                    Popup.IsOpen = false;
                }
                OnClosed(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Should be called by the parent control before the base
        /// OnApplyTemplate method is called.
        /// </summary>
        public void BeforeOnApplyTemplate()
        {
            if (UsesClosingVisualState)
            {
                // Unhook the event handler for the popup closed visual state group.
                // This code is used to enable visual state transitions before 
                // actually setting the underlying Popup.IsOpen property to false.
                VisualStateGroup groupPopupClosed = VisualStates.TryGetVisualStateGroup(Parent, VisualStates.GroupPopup);
                if (null != groupPopupClosed)
                {
                    groupPopupClosed.CurrentStateChanged -= OnPopupClosedStateChanged;
                    UsesClosingVisualState = false;
                }
            }

            if (Popup != null)
            {
                Popup.Closed -= Popup_Closed;
            }
        }

        /// <summary>
        /// Should be called by the parent control after the base
        /// OnApplyTemplate method is called.
        /// </summary>
        public void AfterOnApplyTemplate()
        {
            if (Popup != null)
            {
                Popup.Closed += Popup_Closed;
            }

            var groupPopupClosed = VisualStates.TryGetVisualStateGroup(Parent, VisualStates.GroupPopup);
            if (null != groupPopupClosed)
            {
                groupPopupClosed.CurrentStateChanged += OnPopupClosedStateChanged;
                UsesClosingVisualState = true;
            }

            // TODO: Consider moving to the DropDownPopup setter
            // TODO: Although in line with other implementations, what happens 
            // when the template is swapped out?
            if (Popup != null)
            {
                PopupChild = Popup.Child as FrameworkElement;

                if (PopupChild != null)
                {
                    PopupChild.MinWidth = 203; // TODO: Make this configurable.
                    PopupChild.GotFocus += PopupChild_GotFocus;
                    PopupChild.LostFocus += PopupChild_LostFocus;
                    PopupChild.MouseEnter += PopupChild_MouseEnter;
                    PopupChild.MouseLeave += PopupChild_MouseLeave;
                }
            }
        }

        /// <summary>
        /// Connected to the Popup Closed event and fires the Closed event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void Popup_Closed(object sender, EventArgs e)
        {
            OnClosed(EventArgs.Empty);
        }

        /// <summary>
        /// Connected to several events that indicate that the FocusChanged 
        /// event should bubble up to the parent control.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void OnFocusChanged(EventArgs e)
        {
            EventHandler handler = FocusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Fires the UpdateVisualStates event.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void OnUpdateVisualStates(EventArgs e)
        {
            EventHandler handler = UpdateVisualStates;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// The popup child has received focus.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_GotFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChanged(EventArgs.Empty);
        }

        /// <summary>
        /// The popup child has lost focus.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_LostFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChanged(EventArgs.Empty);
        }

        /// <summary>
        /// The popup child has had the mouse enter its bounds.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_MouseEnter(object sender, MouseEventArgs e)
        {
            OnUpdateVisualStates(EventArgs.Empty);
        }

        /// <summary>
        /// The mouse has left the popup child's bounds.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_MouseLeave(object sender, MouseEventArgs e)
        {
            OnUpdateVisualStates(EventArgs.Empty);
        }
    }
}
