using System;
using System.ComponentModel.Composition;
using GitHub.InlineReviews.Commands;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews
{
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(CommentsMargin.MarginName)]
    [Order(Before = PredefinedMarginNames.ZoomControl)]  // Ensure that the margin occurs below the horizontal scrollbar
    [MarginContainer(PredefinedMarginNames.BottomControl)]             // Set the container to the bottom of the editor window
    [ContentType("text")]                                       // Show this margin for all text-based types
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class CommentsMarginFactory : IWpfTextViewMarginProvider
    {
        readonly IEnableInlineCommentsCommand enableInlineCommentsCommand;

        [ImportingConstructor]
        public CommentsMarginFactory(IEnableInlineCommentsCommand enableInlineCommentsCommand)
        {
            this.enableInlineCommentsCommand = enableInlineCommentsCommand;
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
            return new CommentsMargin(wpfTextViewHost.TextView, enableInlineCommentsCommand);
        }
    }
}
