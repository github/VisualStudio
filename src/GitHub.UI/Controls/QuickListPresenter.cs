using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace GitHub.UI
{
    /// <summary>
    /// Displays a fast virtualized list of items.
    /// </summary>
    /// <remarks>
    /// The standard WPF list controls are slow with large lists even when virtualization is
    /// enabled because they sometimes fail to recycle elements when scrolling. This control is
    /// a very quick (and very limited) virtualized list control designed to prioritize scroll
    /// speed over all else.
    /// 
    /// Some limitations of this control: only vertical layouts, all containers must have the
    /// same height, no data template support, no ItemContainerStyle.
    /// 
    /// To use the control, place it in a `<ScrollViewer CanContentScroll="True">` control and
    /// set the <see cref="ItemContainerType"/> property to the type of the control that will
    /// display the items.
    /// </remarks>
    public class QuickListPresenter : Panel, IScrollInfo
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            ItemsControl.ItemsSourceProperty.AddOwner(
                typeof(QuickListPresenter),
                new FrameworkPropertyMetadata(HandleItemsSourceChanged));

        public static readonly DependencyProperty ItemContainerTypeProperty =
            DependencyProperty.Register(
                nameof(ItemContainerType),
                typeof(Type),
                typeof(QuickListPresenter),
                new FrameworkPropertyMetadata(HandleItemContainerTypeChanged));

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(
                nameof(Spacing),
                typeof(double),
                typeof(QuickListPresenter),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        double itemHeight;
        int overflow;
        int offset;
        bool hiddenLastItem;

        /// <summary>
        /// Gets the type of item container that will be created by the list.
        /// </summary>
        public Type ItemContainerType
        {
            get { return (Type)GetValue(ItemContainerTypeProperty); }
            set { SetValue(ItemContainerTypeProperty, value); }
        }

        /// <summary>
        /// Gets the source of the items to display.
        /// </summary>
        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Gets the spacing to display between items.
        /// </summary>
        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        bool IScrollInfo.CanVerticallyScroll
        {
            get { return true; }
            set { }
        }

        bool IScrollInfo.CanHorizontallyScroll
        {
            get { return false; }
            set { }
        }

        public double ExtentWidth => ActualWidth;
        public double ExtentHeight => ItemCount + overflow;
        public double ViewportWidth => ActualWidth;
        public double ViewportHeight => InternalChildren.Count;
        public double HorizontalOffset => 0;
        public double VerticalOffset => offset;
        ScrollViewer IScrollInfo.ScrollOwner { get; set; }

        int ItemCount => ItemsSource?.Count ?? 0;
        double RowHeight => itemHeight + Spacing;

        public void SetVerticalOffset(double offset)
        {
            var coerced = CoerceOffset(offset);
            var delta = coerced - this.offset;
            this.offset = coerced;
            AssignContainers(delta);
            InvalidateScrollInfo();
        }

        void IScrollInfo.LineUp() => SetVerticalOffset((int)offset - 1);
        void IScrollInfo.LineDown() => SetVerticalOffset(offset + 1);
        void IScrollInfo.PageUp() => SetVerticalOffset(offset - Children.Count);
        void IScrollInfo.PageDown() => SetVerticalOffset(offset + Children.Count);
        void IScrollInfo.MouseWheelUp() => ((IScrollInfo)this).LineUp();
        void IScrollInfo.MouseWheelDown() => ((IScrollInfo)this).LineDown();
        void IScrollInfo.LineLeft() { }
        void IScrollInfo.LineRight() { }
        void IScrollInfo.PageLeft() { }
        void IScrollInfo.PageRight() { }
        void IScrollInfo.MouseWheelLeft() { }
        void IScrollInfo.MouseWheelRight() { }
        void IScrollInfo.SetHorizontalOffset(double offset) { }
        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle) => Rect.Empty;

        protected override Size MeasureOverride(Size availableSize)
        {
            var count = ItemCount;

            if (count > 0)
            {
                if (InternalChildren.Count == 0)
                {
                    InternalChildren.Add(CreateContainer());
                }

                var child = InternalChildren[0];
                child.Measure(availableSize);
                itemHeight = child.DesiredSize.Height;

                return new Size(
                    child.DesiredSize.Width,
                    Math.Min(child.DesiredSize.Height * count, availableSize.Height));
            }
            else
            {
                return Size.Empty;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            CreateContainers(finalSize.Height);
            PositionContainers(finalSize);
            CoerceCurrentOffset();
            AssignContainers(0);
            InvalidateScrollInfo();
            return finalSize;
        }

        int CoerceOffset(double value)
        {
            var max = ExtentHeight - ViewportHeight;
            return (int)Math.Max(0, Math.Min(value, max));
        }

        void CoerceCurrentOffset()
        {
            var coerced = CoerceOffset(offset);

            if (coerced != offset)
            {
                offset = coerced;
                InvalidateScrollInfo();
            }
        }

        FrameworkElement CreateContainer()
        {
            return (FrameworkElement)Activator.CreateInstance(ItemContainerType);
        }

        void CreateContainers(double height)
        {
            if (ItemContainerType != null && itemHeight != 0)
            {
                var count = (int)Math.Min(Math.Ceiling(height / RowHeight), ItemCount);

                while (InternalChildren.Count < count)
                {
                    if (hiddenLastItem)
                    {
                        InternalChildren[InternalChildren.Count - 1].Visibility = Visibility.Visible;
                        hiddenLastItem = false;
                    }

                    var container = CreateContainer();
                    InternalChildren.Add(container);
                }

                if (InternalChildren.Count > count)
                {
                    InternalChildren.RemoveRange(count, InternalChildren.Count - count);
                }
            }
        }

        void PositionContainers(Size size)
        {
            var y = 0.0;

            foreach (FrameworkElement child in InternalChildren)
            {
                child.Arrange(new Rect(0, y, size.Width, RowHeight));
                y += RowHeight;
            }

            overflow = y > size.Height ? 1 : 0;
        }

        void AssignContainers(double delta)
        {
            if (ItemsSource != null)
            {
                var start = offset;
                var pastEnd = offset + ViewportHeight > ItemCount;

                if (!pastEnd && hiddenLastItem)
                {
                    InternalChildren[InternalChildren.Count - 1].Visibility = Visibility.Visible;
                    hiddenLastItem = false;
                }

                // When the scroll delta is 1 item up or down it's better to take the first or last item
                // and move it to the bottom/top. Doing this for more items seems to be slower than just
                // reassigning the DataContext, though I'm not sure where the cutoff point is. However 
                // given that the mouse-wheel and scroll button scroll by 1 item, we'll just special-case
                // the single item.
                if (delta == -1)
                {
                    var child = InternalChildren[InternalChildren.Count - 1];
                    InternalChildren.RemoveAt(InternalChildren.Count - 1);
                    InternalChildren.Insert(0, child);
                    ((FrameworkElement)child).DataContext = ItemsSource[start];
                }
                else if (delta == 1)
                {
                    var child = InternalChildren[0];
                    InternalChildren.RemoveAt(0);
                    InternalChildren.Add(child);

                    if (!pastEnd)
                    {
                        ((FrameworkElement)child).DataContext = ItemsSource[start + InternalChildren.Count - 1];
                    }
                }
                else
                {
                    var index = start;
                    foreach (var child in InternalChildren)
                    {
                        if (index >= ItemCount) break;
                        ((FrameworkElement)child).DataContext = ItemsSource[index++];
                    }
                }

                if (pastEnd)
                {
                    InternalChildren[InternalChildren.Count - 1].Visibility = Visibility.Hidden;
                    hiddenLastItem = true;
                }
            }
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    InvalidateMeasure();
                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    AssignContainers(0);
                    break;
            }

            InvalidateScrollInfo();
        }

        void ItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldIncc = e.OldValue as INotifyCollectionChanged;
            var newIncc = e.NewValue as INotifyCollectionChanged;

            if (oldIncc != null)
            {
                oldIncc.CollectionChanged += CollectionChanged;
            }

            if (newIncc != null)
            {
                newIncc.CollectionChanged += CollectionChanged;
            }

            InvalidateMeasure();
            InvalidateScrollInfo();
        }

        void ItemContainerTypeChanged()
        {
            InternalChildren.Clear();
            CreateContainers(ActualHeight);
        }

        void InvalidateScrollInfo()
        {
            ((IScrollInfo)this).ScrollOwner?.InvalidateScrollInfo();
        }

        static void HandleItemContainerTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as QuickListPresenter)?.ItemContainerTypeChanged();
        }

        static void HandleItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as QuickListPresenter)?.ItemsSourceChanged(e);
        }
    }
}
