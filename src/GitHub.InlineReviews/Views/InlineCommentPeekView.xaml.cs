using System;
using System.Reactive.Subjects;
using System.Windows.Controls;
using GitHub.VisualStudio.UI.Helpers;

namespace GitHub.InlineReviews.Views
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public partial class InlineCommentPeekView : UserControl
    {
        readonly Subject<double> desiredHeight;

        public InlineCommentPeekView()
        {
            InitializeComponent();

            desiredHeight = new Subject<double>();
            threadView.LayoutUpdated += ChildLayoutUpdated;
            annotationsView.LayoutUpdated += ChildLayoutUpdated;
            threadScroller.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
        }

        public IObservable<double> DesiredHeight => desiredHeight;

        void ChildLayoutUpdated(object sender, EventArgs e)
        {
            var otherControlsHeight = ActualHeight - threadScroller.ActualHeight;
            var threadViewHeight = threadView.DesiredSize.Height + threadView.Margin.Top + threadView.Margin.Bottom;
            var annotationsViewHeight = annotationsView.DesiredSize.Height + annotationsView.Margin.Top + annotationsView.Margin.Bottom;
            desiredHeight.OnNext(threadViewHeight + annotationsViewHeight + otherControlsHeight);
        }
    }
}
