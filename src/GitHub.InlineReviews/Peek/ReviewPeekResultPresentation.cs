using System;
using System.Windows;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;
using GitHub.InlineReviews.Views;
using GitHub.Models;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekResultPresentation : IPeekResultPresentation
    {
        readonly IAccount user;
        readonly ReviewPeekResult result;

        public bool IsDirty => false;
        public bool IsReadOnly => true;

        public ReviewPeekResultPresentation(ReviewPeekResult result)
        {
            this.result = result;
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
            var viewModel = new CommentThreadViewModel(result.Session.User, result.Comments);
            var view = new CommentBlockView();
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
