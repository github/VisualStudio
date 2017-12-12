using Microsoft.VisualStudio.Shell;
using System;

namespace GitHub.Services
{
    public interface IVSUIContextFactory
    {
        IVSUIContext GetUIContext(Guid contextGuid);
    }

    public interface IVSUIContextChangedEventArgs
    {
        bool Activated { get; }
    }

    public interface IVSUIContext
    {
        bool IsActive { get; }
        event EventHandler<IVSUIContextChangedEventArgs> UIContextChanged;
    }
}