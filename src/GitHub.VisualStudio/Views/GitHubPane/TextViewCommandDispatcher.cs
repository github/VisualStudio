using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    /// <summary>
    /// Intercepts all commands sent to a <see cref="IVsTextView"/> and fires <see href="Exec"/> when a specified command is encountered.
    /// </summary>
    /// <remarks>
    /// Intercepting commands like this is necessary if we want to capture commands in context of a <see cref="IVsTextView"/>.
    /// Well known commands like Copy, Paste and Enter can be captured as well as custom commands.
    /// </remarks>
    class TextViewCommandDispatcher : IOleCommandTarget, IDisposable
    {
        readonly IVsTextView textView;
        readonly Guid commandGroup;
        readonly int commandId;
        readonly IOleCommandTarget next;

        /// <summary>
        /// Add a command filter to <see cref="IVsTextView"/>.
        /// </summary>
        /// <param name="textView">The text view to filter commands from.</param>
        /// <param name="commandGroup">The group of the command to listen for.</param>
        /// <param name="commandId">The ID of the command to listen for.</param>
        public TextViewCommandDispatcher(IVsTextView textView, Guid commandGroup, int commandId)
        {
            this.textView = textView;
            this.commandGroup = commandGroup;
            this.commandId = commandId;

            ErrorHandler.ThrowOnFailure(textView.AddCommandFilter(this, out next));
        }

        /// <summary>
        /// Remove the command filter.
        /// </summary>
        public void Dispose()
        {
            ErrorHandler.ThrowOnFailure(textView.RemoveCommandFilter(this));
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == commandGroup && nCmdID == commandId)
            {
                Exec?.Invoke(this, EventArgs.Empty);
                return 0;
            }

            return next?.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut) ?? 0;
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == commandGroup)
            {
                if (prgCmds != null && cCmds == 1)
                {
                    if (prgCmds[0].cmdID == commandId)
                    {
                        prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED | (uint)OLECMDF.OLECMDF_ENABLED;
                        return VSConstants.S_OK;
                    }
                }
            }

            return next?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText) ?? 0;
        }

        /// <summary>
        /// Fired when a command of the filtered type is executed.
        /// </summary>
        public event EventHandler Exec;
    }
}
