using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    class TextViewCommandDispatcher : IOleCommandTarget, IDisposable
    {
        readonly IVsTextView textView;
        readonly Guid commandGroup;
        readonly int commandId;
        readonly IOleCommandTarget next;

        public TextViewCommandDispatcher(IVsTextView textView, Guid commandGroup, int commandId)
        {
            this.textView = textView;
            this.commandGroup = commandGroup;
            this.commandId = commandId;

            ErrorHandler.ThrowOnFailure(textView.AddCommandFilter(this, out next));
        }

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
            return next?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText) ?? 0;
        }

        public event EventHandler Exec;
    }
}
