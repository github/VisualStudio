using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.UI;
using GitHub.Extensions;
using System.Diagnostics;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IDynamicMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CreateGist : MenuBase, IDynamicMenuHandler
    {
        readonly Lazy<ISelectedTextProvider> selectedTextProvider;

        [ImportingConstructor]
        public CreateGist([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            Lazy<ISelectedTextProvider> selectedTextProvider)
            : base(serviceProvider)
        {
            this.selectedTextProvider = selectedTextProvider;
        }

        public Guid Guid { get { return GuidList.guidContextMenuSet; } }
        public int CmdId { get { return PkgCmdIDList.createGistCommand; } }

        public bool CanShow()
        {
            var stp = selectedTextProvider.Value;
            Debug.Assert(stp != null, "Could not get an instance of ISelectedTextProvider");
            return !String.IsNullOrWhiteSpace(stp?.GetSelectedText());
        }

        public void Activate(object data)
        {
            StartFlow(UIControllerFlow.Gist);
        }
    }
}
