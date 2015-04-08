using GitHub.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GitHub.UI
{
    public interface IUIController
    {
        //IObservable<object> Transition { get; }
        IObservable<UserControl> SelectFlow(UIControllerFlow choice);
        void Start(IConnection connection);
        void Stop();
        bool IsStopped { get; }
    }

    public enum UIControllerFlow
    {
        None = 0,
        Authentication = 1,
        Create = 2,
        Clone = 3,
        Publish
    }
}
