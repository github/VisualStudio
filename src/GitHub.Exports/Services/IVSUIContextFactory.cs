using System;

namespace GitHub.Services
{
    public interface IVSUIContextFactory
    {
        IVSUIContext GetUIContext(Guid contextGuid);
    }

    public sealed class VSUIContextChangedEventArgs
    {
        public VSUIContextChangedEventArgs(bool activated)
        {
            Activated = activated;
        }

        public bool Activated { get; }
    }

    public interface IVSUIContext
    {
        bool IsActive { get; }
        void WhenActivated(Action action);
    }
}