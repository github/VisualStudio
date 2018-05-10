using System;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Services;
using GitHub.Settings;
using GitHub.InlineReviews.Commands;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Margins
{
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(CommentsMargin.MarginName)]
    [Order(After = PredefinedMarginNames.ZoomControl)]
    [MarginContainer(PredefinedMarginNames.BottomControl)]             // Set the container to the bottom of the editor window
    [ContentType("text")]                                       // Show this margin for all text-based types
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class CommentsMarginFactory : IWpfTextViewMarginProvider
    {
        readonly IPullRequestSessionManager sessionManager;
        readonly IToggleInlineCommentMarginCommand enableInlineCommentsCommand;
        readonly IGoToSolutionOrPullRequestFileCommand goToSolutionOrPullRequestFileCommand;
        readonly IPackageSettings packageSettings;

        [ImportingConstructor]
        public CommentsMarginFactory(
            IToggleInlineCommentMarginCommand enableInlineCommentsCommand,
            IGoToSolutionOrPullRequestFileCommand goToSolutionOrPullRequestFileCommand,
            IPullRequestSessionManager sessionManager,
            IPackageSettings packageSettings)
        {
            this.enableInlineCommentsCommand = enableInlineCommentsCommand;
            this.goToSolutionOrPullRequestFileCommand = goToSolutionOrPullRequestFileCommand;
            this.sessionManager = sessionManager;
            this.packageSettings = packageSettings;
        }

        /// <summary>
        /// Creates an <see cref="IWpfTextViewMargin"/> for the given <see cref="IWpfTextViewHost"/>.
        /// </summary>
        /// <param name="wpfTextViewHost">The <see cref="IWpfTextViewHost"/> for which to create the <see cref="IWpfTextViewMargin"/>.</param>
        /// <param name="marginContainer">The margin that will contain the newly-created margin.</param>
        /// <returns>The <see cref="IWpfTextViewMargin"/>.
        /// The value may be null if this <see cref="IWpfTextViewMarginProvider"/> does not participate for this context.
        /// </returns>
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            // Comments in the editor feature flag
            if (!packageSettings.EditorComments)
            {
                return null;
            }

            // Never show on diff views
            if (IsDiffView(wpfTextViewHost.TextView))
            {
                return null;
            }

            return new CommentsMargin(wpfTextViewHost.TextView, enableInlineCommentsCommand, goToSolutionOrPullRequestFileCommand,
                sessionManager);
        }

        bool IsDiffView(ITextView textView) => textView.Roles.Contains("DIFF");
    }
}
