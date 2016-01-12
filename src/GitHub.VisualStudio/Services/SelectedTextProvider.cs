using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.VisualStudio.Services
{
    [Export(typeof(ISelectedTextProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SelectedTextProvider : ISelectedTextProvider
    {
        readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public SelectedTextProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string GetSelectedText()
        {
            var textManager = serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
            if (textManager == null)
                return string.Empty;

            IVsTextView activeView;
            if (ErrorHandler.Failed(textManager.GetActiveView(1, null, out activeView)))
                return string.Empty;

            string selectedText;
            if (ErrorHandler.Failed(activeView.GetSelectedText(out selectedText)))
                selectedText = string.Empty;

            return selectedText;
        }
    }
}

