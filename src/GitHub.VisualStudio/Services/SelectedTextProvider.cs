using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.VisualStudio
{
    [Export(typeof(ISelectedTextProvider))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
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
            IVsTextManager textManager = serviceProvider.GetService<SVsTextManager, IVsTextManager>();
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

