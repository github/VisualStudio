using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.Services
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

        public IObservable<string> GetSelectedText()
        {
            return Observable.Defer(() =>
            {
                var textManager = serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
                if (textManager == null)
                    return Observable.Return(string.Empty);

                IVsTextView activeView;
                if (ErrorHandler.Failed(textManager.GetActiveView(1, null, out activeView)))
                    return Observable.Return(string.Empty);

                // Maybe we should log here that no text was actually highlighted?
                string highlightedText;
                if (ErrorHandler.Failed(activeView.GetSelectedText(out highlightedText)))
                    highlightedText = string.Empty;

                return Observable.Return(highlightedText);
            });
        }
    }
}

