using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using GitHub.Services;

namespace GitHub.TeamFoundation.Services
{
    [Export(typeof(IVSUIContextFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class VSUIContextFactory : IVSUIContextFactory
    {
        public IVSUIContext GetUIContext(Guid contextGuid)
        {
            return new VSUIContext(UIContext.FromUIContextGuid(contextGuid));
        }
    }

    class VSUIContextChangedEventArgs : IVSUIContextChangedEventArgs
    {
        public bool Activated { get; }

        public VSUIContextChangedEventArgs(bool activated)
        {
            Activated = activated;
        }
    }

    class VSUIContext : IVSUIContext
    {
        readonly UIContext context;
        readonly Dictionary<EventHandler<IVSUIContextChangedEventArgs>, EventHandler<UIContextChangedEventArgs>> handlers =
            new Dictionary<EventHandler<IVSUIContextChangedEventArgs>, EventHandler<UIContextChangedEventArgs>>();
        public VSUIContext(UIContext context)
        {
            this.context = context;
        }

        public bool IsActive { get { return context.IsActive; } }

        public event EventHandler<IVSUIContextChangedEventArgs> UIContextChanged
        {
            add
            {
                EventHandler<UIContextChangedEventArgs> handler = null;
                if (!handlers.TryGetValue(value, out handler))
                {
                    handler = (s, e) => value.Invoke(s, new VSUIContextChangedEventArgs(e.Activated));
                    handlers.Add(value, handler);
                }
                context.UIContextChanged += handler;
            }
            remove
            {
                EventHandler<UIContextChangedEventArgs> handler = null;
                if (handlers.TryGetValue(value, out handler))
                {
                    handlers.Remove(value);
                    context.UIContextChanged -= handler;
                }
            }
        }
    }
}
