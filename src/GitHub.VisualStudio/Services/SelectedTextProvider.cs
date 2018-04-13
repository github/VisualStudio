using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Threading.Tasks;

namespace GitHub.VisualStudio
{
    [Export(typeof(ISelectedTextProvider))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SelectedTextProvider : ISelectedTextProvider
    {
        readonly IVsTextManager textManager;

        [ImportingConstructor]
        public SelectedTextProvider([Import(typeof(SVsTextManager))] IVsTextManager textManager)
        {
            this.textManager = textManager;
        }

        public string GetSelectedText()
        {
            string selectedText;
            IVsTextView activeView;

            if (textManager != null &&
                ErrorHandler.Succeeded(textManager.GetActiveView(1, null, out activeView)) &&
                ErrorHandler.Succeeded(activeView.GetSelectedText(out selectedText)))
                return selectedText;

            return string.Empty;
        }
    }
}

