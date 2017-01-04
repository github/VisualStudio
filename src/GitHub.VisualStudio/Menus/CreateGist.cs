using GitHub.Services;
using GitHub.UI;
using NullGuard;
using System;
using System.Diagnostics;

namespace GitHub.VisualStudio.Menus
{
    public class CreateGist : MenuBase, IDynamicMenuHandler
    {
        readonly Lazy<ISelectedTextProvider> selectedTextProvider;
        ISelectedTextProvider SelectedTextProvider => selectedTextProvider.Value;

        public CreateGist(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            selectedTextProvider = new Lazy<ISelectedTextProvider>(() => ServiceProvider.TryGetService<ISelectedTextProvider>());
        }

        public Guid Guid { get { return GuidList.guidContextMenuSet; } }
        public int CmdId { get { return PkgCmdIDList.createGistCommand; } }

        public bool CanShow()
        {
            Debug.Assert(SelectedTextProvider != null, "Could not get an instance of ISelectedTextProvider");
            return !String.IsNullOrWhiteSpace(SelectedTextProvider?.GetSelectedText());
        }

        public void Activate([AllowNull] object data)
        {
            StartFlow(UIControllerFlow.Gist);
        }
    }
}
