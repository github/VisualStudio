using System;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// Interface for MEF exports implementing menu handlers
    /// (top level context, toolbar, etc)
    /// </summary>
    public interface IMenuHandler
    {
        Guid Guid { get; }
        int CmdId { get; }
        void Activate(object data = null);
    }

    /// <summary>
    /// Interface for MEF exports implementing menu handlers
    /// (top level context, toolbar, etc)
    /// Allows hiding the menu (requires vsct visibility flags)
    /// </summary>
    public interface IDynamicMenuHandler : IMenuHandler
    {
        bool CanShow();
    }
}
