using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using System.Diagnostics;

namespace GitHub.Controllers
{
    [NullGuard(ValidationFlags.None)]
    public class NavigationController : NotificationAwareObject, IDisposable, IHasBusy
    {
        readonly List<IUIController> history = new List<IUIController>();
        readonly Dictionary<UIControllerFlow, IUIController> reusableControllers = new Dictionary<UIControllerFlow, IUIController>();
        readonly IUIProvider uiProvider;

        int current = -1;

        public bool HasBack => current > 0;
        public bool HasForward => current < history.Count - 1;
        public IUIController Current => current >= 0 ? history[current] : null;

        readonly CompositeDisposable disposablesForCurrentView = new CompositeDisposable();

        int Pointer
        {
            get
            {
                return current;
            }
            set
            {
                if (current == value)
                    return;

                bool raiseBack = false, raiseForward = false;
                if ((value == 0 && HasBack) || (value > 0 && !HasBack))
                    raiseBack = true;
                if ((value == history.Count - 1 && !HasForward) || (value < history.Count - 1 && HasForward))
                    raiseForward = true;
                current = value;
                this.RaisePropertyChanged(nameof(Current));
                if (raiseBack) this.RaisePropertyChanged(nameof(HasBack));
                if (raiseForward) this.RaisePropertyChanged(nameof(HasForward));
            }
        }

        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; this.RaisePropertyChanged(); }
        }

        public NavigationController(IUIProvider uiProvider)
        {
            this.uiProvider = uiProvider;
        }

        public void LoadView(IConnection connection, ViewWithData data, Action<IView> onViewLoad)
        {
            switch (data.MainFlow)
            {
                case UIControllerFlow.PullRequestCreation:
                    if (data.Data == null && Current?.SelectedFlow == UIControllerFlow.PullRequestCreation)
                    {
                        Reload();
                    }
                    else
                    {
                        CreateView(connection, data, onViewLoad);
                    }
                    break;

                case UIControllerFlow.PullRequestDetail:
                    if (data.Data == null && Current?.SelectedFlow == UIControllerFlow.PullRequestDetail)
                    {
                        Reload();
                    }
                    else
                    {
                        CreateView(connection, data, onViewLoad);
                    }
                    break;

                case UIControllerFlow.PullRequestList:
                case UIControllerFlow.Home:
                default:
                    if (data.MainFlow == Current?.SelectedFlow)
                    {
                        Reload();
                    }
                    else
                    {
                        CreateOrReuseView(connection, data, onViewLoad);
                    }
                    break;
            }
        }

        /// <summary>
        /// Existing views are not reused
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="onViewLoad"></param>
        void CreateView(IConnection connection, ViewWithData data, Action<IView> onViewLoad)
        {
            IsBusy = true;

            var controller = CreateController(connection, data, onViewLoad);
            Push(controller);
        }

        void CreateOrReuseView(IConnection connection, ViewWithData data, Action<IView> onViewLoad)
        {
            IUIController controller;
            var exists = reusableControllers.TryGetValue(data.MainFlow, out controller);

            if (!exists)
            {
                IsBusy = true;

                Action<IView> handler = view =>
                {
                    disposablesForCurrentView?.Clear();

                    var action = view.ViewModel as ICanNavigate;
                    if (action != null)
                    {
                        disposablesForCurrentView.Add(action?.Navigate.Subscribe(d =>
                        {
                            LoadView(connection, d, onViewLoad);
                        }));
                    }
                    onViewLoad?.Invoke(view);
                };

                controller = CreateController(connection, data, handler);
                reusableControllers.Add(data.MainFlow, controller);
            }

            Push(controller);

            if (exists)
            {
                Reload();
            }
        }

        public void Reload()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            Current?.Reload();
        }

        public void Back()
        {
            if (!HasBack)
                return;
            Pointer--;
            Reload();
        }

        public void Forward()
        {
            if (!HasForward)
                return;
            Pointer++;
            Reload();
        }

        IUIController CreateController(IConnection connection, ViewWithData data, Action<IView> onViewLoad)
        {
            var controller = uiProvider.Configure(data.MainFlow, connection, data);
            controller.TransitionSignal.Subscribe(
                loadData =>
                {
                    onViewLoad?.Invoke(loadData.View);
                    IsBusy = false;
                },
                () => {
                    Pop(controller);
                    Reload();
                });
            controller.Start();
            return controller;
        }

        void Push(IUIController controller)
        {
            while (history.Count > Pointer + 1)
            {
                history.RemoveAt(history.Count - 1);
            }

            history.Add(controller);
            Pointer++;
        }

        void Pop(IUIController controller = null)
        {
            var c = current;
            controller = controller ?? history[history.Count - 1];
            var count = history.Count;
            for (int i = 0; i < count; i++)
            {
                if (history[i] == controller)
                {
                    history.RemoveAt(i);
                    if (i <= c)
                        c--;
                    i--;
                    count--;
                }
            }
            reusableControllers.Remove(controller.SelectedFlow);
            controller.Stop();
            Pointer = c;
        }

        bool disposed = false;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                    disposablesForCurrentView.Dispose();
                    reusableControllers.Values.ForEach(c => uiProvider.StopUI(c));
                    reusableControllers.Clear();
                    history.Clear();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
