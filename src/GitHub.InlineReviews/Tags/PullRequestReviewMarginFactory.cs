using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using GitHub.Services;

namespace ScratchMargin
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(PullRequestReviewMargin.MarginName)]
    [Order(Before = PredefinedMarginNames.LineNumber)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class PullRequestReviewMarginFactory : IWpfTextViewMarginProvider
    {
        IPullRequestReviewSession session;

        [ImportingConstructor]
        public PullRequestReviewMarginFactory(IPullRequestReviewSessionManager sessionManager)
        {
            sessionManager.CurrentSession.Subscribe(s =>
            {
                session = s;
            });
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            if(session == null)
            {
                return null;
            }

            return new PullRequestReviewMargin(wpfTextViewHost);
        }
    }
}
