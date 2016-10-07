using GitHub.Exports;
using GitHub.Models;
using System;

namespace GitHub.UI
{
    public interface IUIController
    {
        IObservable<LoadData> SelectFlow(UIControllerFlow choice);
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
        UIControllerFlow SelectedFlow { get; }
        void Jump(ViewWithData where);
    }

    public enum UIControllerFlow
    {
        None = 0,
        Authentication,
        Create,
        Clone,
        Publish,
        PullRequests,
        Gist,
        LogoutRequired,
        Home
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

    public enum LoadDirection
    {
        None,
        Forward,
        Back
    }

    public struct LoadData
    {
        public IView View;
        public ViewWithData Data;
        public LoadDirection Direction;

        public override int GetHashCode()
        {
            return (View?.GetHashCode() ?? 0) ^ (Data?.GetHashCode() ?? 0) ^ Direction.GetHashCode();
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
