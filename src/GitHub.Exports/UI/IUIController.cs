using System;
using System.Windows.Controls;

namespace GitHub.UI
{
    public interface IUIController
    {
        //IObservable<object> Transition { get; }
        IObservable<UserControl> SelectFlow(UIControllerFlow choice);
        void Start();
    }

    public enum UIControllerFlow
    {
        None = 0,
        Authentication = 1,
        Create = 2,
        Clone = 3
    }
}
