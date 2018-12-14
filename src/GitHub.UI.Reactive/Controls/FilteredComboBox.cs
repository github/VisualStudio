using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GitHub.Extensions.Reactive;
using ReactiveUI;

namespace GitHub.UI
{
    public class FilteredComboBox : ComboBox
    {
        TextBox filterTextBox;

        public FilteredComboBox()
        {
            IsTextSearchEnabled = true;

            this.WhenAny(x => x.FilterText, x => x.Value)
                .WhereNotNull()
                .Throttle(TimeSpan.FromSeconds(0.2), RxApp.MainThreadScheduler)
                .Subscribe(filterText =>
                {
                    Items.Filter += DoesItemStartWith(filterText);
                });

            this.WhenAny(x => x.FilterText, x => x.Value)
                .Where(x => x == null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(filterText =>
                {
                    Items.Filter += x => true;
                });

            this.WhenAny(x => x.FilterText, x => x.Value)
                .Select(x => !string.IsNullOrEmpty(x))
                .Subscribe(isFiltered =>
                {
                    IsListFiltered = isFiltered;
                });
        }

        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register("FilterText", typeof(string), typeof(FilteredComboBox));

        public string FilterText
        {
            get { return (string)GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }

        public static readonly DependencyProperty IsListFilteredProperty = DependencyProperty.Register("IsListFiltered", typeof(bool), typeof(FilteredComboBox));

        public bool IsListFiltered
        {
            get { return (bool)GetValue(IsListFilteredProperty); }
            set { SetValue(IsListFilteredProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            filterTextBox = GetTemplateChild("PART_FilterTextBox") as TextBox;
            var popUp = GetTemplateChild("PART_Popup") as Popup;
            if (popUp != null)
            {
                popUp.CustomPopupPlacementCallback = PlacePopup;
            }
            base.OnApplyTemplate();
        }

        /// <summary>
        /// Override selection changed, because when the ItemSource is filtered
        /// selection change is triggered, and the old selection is lost.
        /// This allows us to remember the previous selection when no new selection has been made
        /// and prevent a selection of null, when we had a previous selection.
        /// </summary>
        /// <param name="e">The selection changed arguments</param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            var hasOldSelection = e.RemovedItems != null && e.RemovedItems.Count == 1;
            var hasNewSelectedItem = e.AddedItems != null && e.AddedItems.Count == 1;

            if (hasOldSelection && !hasNewSelectedItem)
            {
                return;
            }
            base.OnSelectionChanged(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (filterTextBox != null && char.IsLetterOrDigit((char)e.Key))
            {
                filterTextBox.Focus();
                Keyboard.Focus(filterTextBox);
            }
            else if (e.Key == Key.Enter && IsListFiltered && Items.Count > 0)
            {
                SelectedItem = Items.GetItemAt(0);
                IsDropDownOpen = false;
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            FilterText = "";
            SelectedIndex = -1;
            base.OnDropDownOpened(e);
        }   

        Predicate<object> DoesItemStartWith(string filterText)
        {
            return item =>
            {
                var text = TryGetSearch(item);

                var comparison = IsTextSearchCaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                return text.StartsWith(filterText, comparison);
            };
        }

        public static CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            return new[] { new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Vertical) };
        }

        string TryGetSearch(object item)
        {
            string text = string.Empty;
            var textPath = TextSearch.GetTextPath(this);
            var propertyInfo = item.GetType().GetProperty(textPath);
            if (propertyInfo != null)
            {
                text = propertyInfo.GetValue(item).ToString();
            }
            return text;
        }
    }
}
