using System;
using Microsoft.VisualStudio.Shell;
using GitHub.Services;

namespace GitHub.TeamFoundation.Services
{
    class VSUIContextFactory : IVSUIContextFactory
    {
        public IVSUIContext GetUIContext(Guid contextGuid)
        {
            return new VSUIContext(UIContext.FromUIContextGuid(contextGuid));
        }
    }

    class VSUIContext : IVSUIContext
    {
        readonly UIContext context;

        public VSUIContext(UIContext context)
        {
            this.context = context;
        }

        public bool IsActive { get { return context.IsActive; } }

        public void WhenActivated(Action action) => context.WhenActivated(action);
    }
}
