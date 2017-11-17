using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;

namespace GitHub.Services
{
    public interface IVSUIContextFactory
    {
        IVSUIContext GetUIContext(Guid contextGuid);
    }

    [Export(typeof(IVSUIContextFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSUIContextFactory : IVSUIContextFactory
    {
        public IVSUIContext GetUIContext(Guid contextGuid)
        {
            return new VSUIContext(UIContext.FromUIContextGuid(contextGuid));
        }
    }

    public interface IVSUIContext
    {
        bool IsActive { get; }
        event EventHandler<UIContextChangedEventArgs> UIContextChanged;
    }

    public class VSUIContext : IVSUIContext
    {
        readonly UIContext context;

        public VSUIContext(UIContext context)
        {
            this.context = context;
        }

        public bool IsActive { get { return context.IsActive; } }

        public event EventHandler<UIContextChangedEventArgs> UIContextChanged
        {
            add
            {
                context.UIContextChanged += value;
            }
            remove
            {
                context.UIContextChanged -= value;
            }
        }
    }
}