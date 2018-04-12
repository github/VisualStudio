using System;
using System.Windows;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Editor;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;
using GitHub.InlineReviews.Commands;

namespace GitHub.InlineReviews
{
    /// <summary>
    /// Margin's canvas and visual definition including both size and content
    /// </summary>
    internal class CommentsMargin : IWpfTextViewMargin
    {
        /// <summary>
        /// Margin name.
        /// </summary>
        public const string MarginName = "CommentsMargin";

        readonly IWpfTextView textView;
        readonly CommentsMarginViewModel viewModel;
        readonly CommentsMarginView visualElement;

        /// <summary>
        /// A value indicating whether the object is disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleCommentsMargin"/> class for a given <paramref name="textView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public CommentsMargin(IWpfTextView textView, IEnableInlineCommentsCommand enableInlineCommentsCommand,
            IPullRequestSessionManager sessionManager)
        {
            this.textView = textView;

            viewModel = new CommentsMarginViewModel(sessionManager, textView.TextBuffer, enableInlineCommentsCommand);
            visualElement = new CommentsMarginView { DataContext = viewModel, ClipToBounds = true };

            RefreshMarginEnabled();
            textView.Options.OptionChanged += (s, e) => RefreshMarginEnabled();
        }

        void RefreshMarginEnabled()
        {
            viewModel.MarginEnabled = textView.Options.GetOptionValue<bool>(InlineCommentMarginEnabled.OptionName);
        }

        /// <summary>
        /// Gets the <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation of the margin.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                ThrowIfDisposed();
                return visualElement;
            }
        }

        /// <summary>
        /// Gets the size of the margin.
        /// </summary>
        /// <remarks>
        /// For a horizontal margin this is the height of the margin,
        /// since the width will be determined by the <see cref="ITextView"/>.
        /// For a vertical margin this is the width of the margin,
        /// since the height will be determined by the <see cref="ITextView"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public double MarginSize
        {
            get
            {
                ThrowIfDisposed();

                // Since this is a horizontal margin, its width will be bound to the width of the text view.
                // Therefore, its size is its height.
                return visualElement.ActualHeight;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the margin is enabled.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public bool Enabled
        {
            get
            {
                ThrowIfDisposed();

                // The margin should always be enabled
                return true;
            }
        }

        /// <summary>
        /// Gets the <see cref="ITextViewMargin"/> with the given <paramref name="marginName"/> or null if no match is found
        /// </summary>
        /// <param name="marginName">The name of the <see cref="ITextViewMargin"/></param>
        /// <returns>The <see cref="ITextViewMargin"/> named <paramref name="marginName"/>, or null if no match is found.</returns>
        /// <remarks>
        /// A margin returns itself if it is passed its own name. If the name does not match and it is a container margin, it
        /// forwards the call to its children. Margin name comparisons are case-insensitive.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="marginName"/> is null.</exception>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, CommentsMargin.MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        /// <summary>
        /// Disposes an instance of <see cref="ToggleCommentsMargin"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                GC.SuppressFinalize(this);
                isDisposed = true;
            }
        }

        /// <summary>
        /// Checks and throws <see cref="ObjectDisposedException"/> if the object is disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(MarginName);
            }
        }
    }
}
