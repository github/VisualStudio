using GitHub.Exports;
using GitHub.Models;
using System;
using GitHub.ViewModels;

namespace GitHub.UI
{
    public interface IUIController : IDisposable
    {
        /// <summary>
        /// Allows listening to the completion state of the ui flow - whether
        /// it was completed because it was cancelled or whether it succeeded.
        /// </summary>
        /// <returns>true for success, false for cancel</returns>
        IObservable<bool> ListenToCompletionState();
        void Start();
        void Stop();
        bool IsStopped { get; }
        UIControllerFlow CurrentFlow { get; }
        UIControllerFlow SelectedFlow { get; }
        IObservable<LoadData> TransitionSignal { get; }

        IObservable<LoadData> Configure(UIControllerFlow choice, IConnection connection = null, ViewWithData parameters = null);
        void Reload();
    }

    public enum UIControllerFlow
    {
        None = 0,
        Authentication,
        Create,
        Clone,
        Publish,
        Gist,
        LogoutRequired,
        Home,
        ReClone,
        PullRequestList,
        PullRequestDetail,
        PullRequestCreation,
    }

    public class ViewWithData
    {
        public UIControllerFlow ActiveFlow;
        public UIControllerFlow MainFlow;
        public UIViewType ViewType;
        public object Data;

        public ViewWithData() {}
        public ViewWithData(UIControllerFlow flow)
        {
            ActiveFlow = flow;
            MainFlow = flow;
        }
    }

    public struct LoadData
    {
        public UIControllerFlow Flow;
        public IView View;
        public ViewWithData Data;

        public override int GetHashCode()
        {
            return 17 * (23 + Flow.GetHashCode()) * (23 + (View?.GetHashCode() ?? 0)) * (23 + (Data?.GetHashCode() ?? 0));
        }

        public override bool Equals(object obj)
        {
            if (obj is LoadData)
                return GetHashCode() == obj.GetHashCode();
            return base.Equals(obj);
        }

        public static bool operator==(LoadData lhs, LoadData rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(LoadData lhs, LoadData rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
