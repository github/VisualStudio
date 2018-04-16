using System;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.Reactive.Linq;
using GitHub.Models;
using GitHub.Commands;
using GitHub.Services;
using GitHub.Extensions;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;
using GitHub.InlineReviews.Commands;
using Microsoft.VisualStudio.Text.Editor;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;

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
        readonly IPullRequestSessionManager sessionManager;

        /// <summary>
        /// A value indicating whether the object is disposed.
        /// </summary>
        bool isDisposed;

        IDisposable currentSessionSubscription;
        IDisposable optionChangedSubscription;
        IDisposable visibilitySubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleCommentsMargin"/> class for a given <paramref name="textView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public CommentsMargin(
            IWpfTextView textView,
            IEnableInlineCommentsCommand enableInlineCommentsCommand,
            INextInlineCommentCommand nextInlineCommentCommand,
            IPullRequestSessionManager sessionManager)
        {
            this.textView = textView;
            this.sessionManager = sessionManager;

            viewModel = new CommentsMarginViewModel(enableInlineCommentsCommand, nextInlineCommentCommand);
            visualElement = new CommentsMarginView { DataContext = viewModel, ClipToBounds = true };

            visibilitySubscription = viewModel.WhenAnyValue(x => x.Enabled).Subscribe(enabled =>
            {
                visualElement.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
            });

            optionChangedSubscription = Observable.FromEventPattern(textView.Options, nameof(textView.Options.OptionChanged)).Subscribe(_ =>
            {
                viewModel.MarginEnabled = textView.Options.GetOptionValue<bool>(InlineCommentMarginEnabled.OptionName);
            });

            currentSessionSubscription = sessionManager.WhenAnyValue(x => x.CurrentSession)
                .Subscribe(x => RefreshCurrentSession().Forget());
        }

        async Task RefreshCurrentSession()
        {
            var sessionFile = await FindSessionFile();
            if (sessionFile != null)
            {
                viewModel.FileName = Path.GetFileName(sessionFile.RelativePath);
                viewModel.CommentsInFile = sessionFile.InlineCommentThreads?.Count ?? -1;
                viewModel.Enabled = sessionFile.Diff.Count > 0;
            }
            else
            {
                viewModel.CommentsInFile = 0;
                viewModel.Enabled = false;
            }
        }

        async Task<IPullRequestSessionFile> FindSessionFile()
        {
            await sessionManager.EnsureInitialized();

            var session = sessionManager.CurrentSession;
            if (session == null)
            {
                return null;
            }

            var relativePath = sessionManager.GetRelativePath(textView.TextBuffer);
            if (relativePath == null)
            {
                return null;
            }

            return await session.GetFile(relativePath);
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

                return viewModel.Enabled;
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
                currentSessionSubscription.Dispose();
                optionChangedSubscription.Dispose();
                visibilitySubscription.Dispose();

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
