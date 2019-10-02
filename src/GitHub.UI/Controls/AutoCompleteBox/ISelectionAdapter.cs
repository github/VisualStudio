// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Collections;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHub.UI
{
    /// <summary>
    /// Defines an item collection, selection members, and key handling for the
    /// selection adapter contained in the drop-down portion of an
    /// <see cref="T:Chat.AutoCompleteBox" /> control.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public interface ISelectionAdapter
    {
        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The currently selected item.</value>
        object SelectedItem { get; set; }

        /// <summary>
        /// Occurs when the
        /// <see cref="ISelectionAdapter.SelectedItem" />
        /// property value changes.
        /// </summary>
        event SelectionChangedEventHandler SelectionChanged;
        
        /// <summary>
        /// Gets or sets a collection that is used to generate content for the
        /// selection adapter.
        /// </summary>
        /// <value>The collection that is used to generate content for the
        /// selection adapter.</value>
        IEnumerable ItemsSource { get; set; }

        /// <summary>
        /// Occurs when a selected item is not cancelled and is committed as the
        /// selected item.
        /// </summary>
        event RoutedEventHandler Commit;

        /// <summary>
        /// Occurs when a selection has been canceled.
        /// </summary>
        event RoutedEventHandler Cancel;

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.KeyDown" /> event that occurs
        /// when a key is pressed while the drop-down portion of the
        /// <see cref="T:Chat.AutoCompleteBox" /> has focus.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.KeyEventArgs" />
        /// that contains data about the
        /// <see cref="E:System.Windows.UIElement.KeyDown" /> event.</param>
        void HandleKeyDown(KeyEventArgs e);

        /// <summary>
        /// Returns an automation peer for the selection adapter, for use by the
        /// Silverlight automation infrastructure.
        /// </summary>
        /// <returns>An automation peer for the selection adapter, if one is
        /// available; otherwise, null.</returns>
        AutomationPeer CreateAutomationPeer();
    }
}