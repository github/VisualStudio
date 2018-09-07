using System;
using System.Windows;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;
using GitHub.InlineReviews.Views;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekResultPresentation : IPeekResultPresentation, IDesiredHeightProvider
    {
        const double PeekBorders = 28.0;
        readonly InlineReviewPeekViewModel viewModel;
        InlineReviewPeekView view;
        double desiredHeight;

        public bool IsDirty => false;
        public bool IsReadOnly => true;

        public InlineCommentPeekResultPresentation(InlineReviewPeekViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public double ZoomLevel
        {
            get { return 1.0; }
            set { }
        }

        public double DesiredHeight
        {
            get { return desiredHeight; }
            private set
            {
                if (desiredHeight != value && DesiredHeightChanged != null)
                {
                    desiredHeight = value;
                    DesiredHeightChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler IsDirtyChanged
        {
            add { }
            remove { }
        }

        public event EventHandler IsReadOnlyChanged
        {
            add { }
            remove { }
        }

        public event EventHandler<RecreateContentEventArgs> RecreateContent = delegate { };
        public event EventHandler<EventArgs> DesiredHeightChanged;

        public bool CanSave(out string defaultPath)
        {
            defaultPath = null;
            return false;
        }

        public IPeekResultScrollState CaptureScrollState()
        {
            return null;
        }

        public void Close()
        {
        }

        public UIElement Create(IPeekSession session, IPeekResultScrollState scrollState)
        {
            view = new InlineReviewPeekView();
            view.DataContext = viewModel;

            // Report the desired size back to the peek view. Unfortunately the peek view
            // helpfully assigns this desired size to the control that also contains the tab at
            // the top of the peek view, so we need to put in a fudge factor. Using a const
            // value for the moment, as there's no easy way to get the size of the control.
            view.DesiredHeight.Subscribe(x => DesiredHeight = x + PeekBorders);

            return view;
        }

        public void Dispose()
        {
        }

        public void ScrollIntoView(IPeekResultScrollState scrollState)
        {
        }

        public void SetKeyboardFocus()
        {
        }

        public bool TryOpen(IPeekResult otherResult) => false;

        public bool TryPrepareToClose() => true;

        public bool TrySave(bool saveAs) => true;
    }
}
