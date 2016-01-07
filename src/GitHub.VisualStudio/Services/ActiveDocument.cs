using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Media.TextFormatting;

namespace GitHub.VisualStudio
{
    [Export(typeof(IActiveDocument))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class ActiveDocument : IActiveDocument
    {
        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public int AnchorLine { get; private set; }
        public int AnchorColumn { get; private set; }
        public int EndLine { get; private set; }
        public int EndColumn { get; private set; }

        [ImportingConstructor]
        public ActiveDocument([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            AnchorLine = AnchorColumn = EndLine = EndColumn = -1;
            Name = Services.Dte2?.ActiveDocument?.FullName;
            ShortName = Services.Dte2?.ActiveDocument?.Name;

            var textManager = serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
            Debug.Assert(textManager != null, "No SVsTextManager service available");

            IVsTextView view;
            int anchorLine, anchorCol, endLine, endCol;
            if (ErrorHandler.Succeeded(textManager.GetActiveView(0, null, out view)) &&
                ErrorHandler.Succeeded(view.GetSelection(out anchorLine, out anchorCol, out endLine, out endCol)))
            {
                AnchorLine = anchorLine + 1;
                AnchorColumn = anchorCol + 1;
                EndLine = endLine + 1;
                EndColumn = endCol + 1;
            }
        }
    }
}
