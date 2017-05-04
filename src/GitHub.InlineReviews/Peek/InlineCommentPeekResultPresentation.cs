using System;
using System.Windows;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;
using GitHub.InlineReviews.Views;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekResultPresentation : IPeekResultPresentation
    {
        readonly InlineCommentThreadViewModel viewModel;

        public bool IsDirty => false;
        public bool IsReadOnly => true;

        public InlineCommentPeekResultPresentation(InlineCommentThreadViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public double ZoomLevel
        {
            get { return 1.0; }
            set { }
        }

        public event EventHandler IsDirtyChanged;
        public event EventHandler IsReadOnlyChanged;
        public event EventHandler<RecreateContentEventArgs> RecreateContent;

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
            var view = new CommentThreadView();
            view.DataContext = viewModel;
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
