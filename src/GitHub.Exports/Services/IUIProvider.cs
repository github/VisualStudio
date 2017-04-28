using GitHub.Exports;
using GitHub.Models;
using GitHub.UI;
using GitHub.VisualStudio;
using System;
using System.Runtime.InteropServices;

namespace GitHub.Services
{
    [Guid(Guids.UIProviderId)]
    public interface IUIProvider
    {
        IUIController Configure(UIControllerFlow flow, IConnection connection = null, ViewWithData data = null);
        IUIController Run(UIControllerFlow flow);
        void RunInDialog(UIControllerFlow flow, IConnection connection = null);
        void RunInDialog(IUIController controller);
        IView GetView(UIViewType which, ViewWithData data = null);
        void StopUI(IUIController controller);
        void Run(IUIController controller);
    }
}