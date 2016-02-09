using GitHub.Extensions;
using GitHub.Primitives;
using System;

namespace GitHub.VisualStudio.Menus
{
    public class LinkMenuBase: MenuBase
    {
        public LinkMenuBase(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected UriString GenerateLink()
        {
            var repo = ActiveRepo;
            var activeDocument = ServiceProvider.GetExportedValue<IActiveDocumentSnapshot>();
            if (activeDocument == null)
                return null;
            return repo.GenerateUrl(activeDocument.Name, activeDocument.StartLine, activeDocument.EndLine);
        }
    }
}
