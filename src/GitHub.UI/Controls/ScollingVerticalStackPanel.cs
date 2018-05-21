using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace GitHub.UI.Controls
{
    /// <summary>
    /// A vertical stack panel which implements its own logical scrolling, allowing controls to be
    /// fixed horizontally in the scroll area.
    /// </summary>
    /// <remarks>
    /// This panel is needed by the PullRequestDetailsView because of #1698: there is no default
    /// panel in WPF which allows the horizontal scrollbar to always be present at the bottom while
    /// also making the PR description etc be fixed horizontally (non-scrollable) in the viewport.
    /// </remarks>
    public class ScollingVerticalStackPanel : Panel, IScrollInfo
    {
        const int lineSize = 16;
        const int mouseWheelSize = 48;

        /// <summary>
        /// Attached property which when set to True on a child control, will cause it to be fixed
        /// horizontally within the scrollable viewport.
        /// </summary>
        public static readonly DependencyProperty IsFixedProperty =
            DependencyProperty.RegisterAttached(
                "IsFixed", 
                typeof(bool), 
                typeof(ScollingVerticalStackPanel),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool CanHorizontallyScroll
        {
            get { return true; }
            set { }
        }

        public bool CanVerticallyScroll
        {
            get { return true; }
            set { }
        }

        public double ExtentHeight { get; private set; }
        public double ExtentWidth { get; private set; }
        public double HorizontalOffset { get; private set; }
        public double VerticalOffset { get; private set; }
        public double ViewportHeight { get; private set; }
        public double ViewportWidth { get; private set; }
        public ScrollViewer ScrollOwner { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Can only be applied to controls")]
        public static bool GetIsFixed(FrameworkElement control)
        {
            return (bool)control.GetValue(IsFixedProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Can only be applied to controls")]
        public static void SetIsFixed(FrameworkElement control, bool value)
        {
            control.SetValue(IsFixedProperty, value);
        }

        public void LineDown() => SetVerticalOffset(VerticalOffset + lineSize);
        public void LineLeft() => SetHorizontalOffset(HorizontalOffset - lineSize);
        public void LineRight() => SetHorizontalOffset(HorizontalOffset + lineSize);
        public void LineUp() => SetVerticalOffset(VerticalOffset - lineSize);
        public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + mouseWheelSize);
        public void MouseWheelLeft() => SetHorizontalOffset(HorizontalOffset - mouseWheelSize);
        public void MouseWheelRight() => SetHorizontalOffset(HorizontalOffset + mouseWheelSize);
        public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - mouseWheelSize);
        public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);
        public void PageLeft() => SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        public void PageRight() => SetHorizontalOffset(HorizontalOffset + ViewportWidth);
        public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var transform = visual.TransformToVisual(this);
            var rect = transform.TransformBounds(rectangle);
            var offsetX = HorizontalOffset;
            var offsetY = VerticalOffset;

            if (rect.Bottom > ViewportHeight)
            {
                var delta = rect.Bottom - ViewportHeight;
                offsetY += delta;
                rect.Y -= delta;
            }

            if (rect.Y < 0)
            {
                offsetY += rect.Y;
            }

            // We technially should be trying to also show the right-hand side of the rect here
            // using the same technique that we just used to show the bottom of the rect above,
            // but in the case of the PR details view, the left hand side of the item is much
            // more important than the right hand side and it actually feels better to not do
            // this. If this control is used elsewhere and this behavior is required, we could
            // put in a switch to enable it.

            if (rect.X < 0)
            {
                offsetX += rect.X;
            }

            SetHorizontalOffset(offsetX);
            SetVerticalOffset(offsetY);

            return rect;
        }

        public void SetHorizontalOffset(double offset)
        {
            var value = Math.Max(0, Math.Min(offset, ExtentWidth - ViewportWidth));

            if (value != HorizontalOffset)
            {
                HorizontalOffset = value;
                InvalidateArrange();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            var value = Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));

            if (value != VerticalOffset)
            {
                VerticalOffset = value;
                InvalidateArrange();
            }
        }

        protected override void ParentLayoutInvalidated(UIElement child)
        {
            base.ParentLayoutInvalidated(child);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var maxWidth = 0.0;
            var height = 0.0;

            foreach (FrameworkElement child in Children)
            {
                var isFixed = GetIsFixed(child);
                var childConstraint = new Size(
                    isFixed ? availableSize.Width : double.PositiveInfinity,
                    double.PositiveInfinity);
                child.Measure(childConstraint);
                maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
                height += child.DesiredSize.Height;
            }

            UpdateScrollInfo(new Size(maxWidth, height), availableSize);

            return new Size(
                Math.Min(maxWidth, availableSize.Width),
                Math.Min(height, availableSize.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var y = -VerticalOffset;

            foreach (FrameworkElement child in Children)
            {
                var isFixed = GetIsFixed(child);
                var x = isFixed ? 0 : -HorizontalOffset;
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
                y += child.DesiredSize.Height;
            }

            UpdateScrollInfo(new Size(ExtentWidth, ExtentHeight), finalSize);
            return finalSize;
        }

        void UpdateScrollInfo(Size extent, Size viewport)
        {
            ExtentWidth = extent.Width;
            ExtentHeight = extent.Height;
            ViewportWidth = viewport.Width;
            ViewportHeight = viewport.Height;
            ScrollOwner?.InvalidateScrollInfo();
        }
    }
}
