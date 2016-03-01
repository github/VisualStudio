using GitHub.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GitHub.UI
{
    public interface IUIController
    {
        IObservable<IView> SelectFlow(UIControllerFlow choice);
        /// <summary>
        /// Allows listening to the completion state of the ui flow - whether
        /// it was completed because it was cancelled or whether it succeeded.
        /// </summary>
        /// <returns>true for success, false for cancel</returns>
        IObservable<bool> ListenToCompletionState();
        void Start(IConnection connection);
        void Stop();
        bool IsStopped { get; }
        UIControllerFlow CurrentFlow { get; }
    }

    public enum UIControllerFlow
    {
        None = 0,
        Authentication,
        Create,
        Clone,
        Publish,
        PullRequests
    }
}
