using System.Threading.Tasks;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using GitHub.Extensions;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using System.Windows;
using static Microsoft.VisualStudio.Threading.JoinableTaskFactory;
using static Microsoft.VisualStudio.Threading.AwaitExtensions;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Helpers
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }
        void GetResult();
    }

    public static class ThreadingHelper
    {
        public static bool InUIThread => Guard.InUnitTestRunner ? true : Application.Current.Dispatcher.CheckAccess();

        /// <summary>
        /// Gets the Dispatcher for the main thread.
        /// </summary>
        public static Dispatcher MainThreadDispatcher => Application.Current.Dispatcher;

        /// <summary>
        /// Switch to the UI thread using ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync
        /// Auto-disables switching when running in unit test mode
        /// </summary>
        /// <returns></returns>
        public static IAwaitable SwitchToMainThreadAsync()
        {
            return Guard.InUnitTestRunner ?
                new AwaitableWrapper() :
                new AwaitableWrapper(ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync());
        }

        /// <summary>
        /// Switch to a thread pool background thread if the current thread isn't one, otherwise does nothing
        /// Auto-disables switching when running in unit test mode
        /// </summary>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static IAwaitable SwitchToPoolThreadAsync(TaskScheduler scheduler = null)
        {
            return Guard.InUnitTestRunner ?
                new AwaitableWrapper() :
                new AwaitableWrapper(scheduler ?? TaskScheduler.Default);
        }

        // HACK: This is a workaround because the following doesn't seem to work.
        //await JoinableTaskFactory
        //   .WithPriority(VsTaskRunContext.UIThreadNormalPriority)
        //   .SwitchToMainThreadAsync();
        public static Task RunOnMainThreadNormalPriority(Action action)
        {
            var service = (IVsTaskSchedulerService2)VsTaskLibraryHelper.ServiceInstance;
            var scheduler = service.GetTaskScheduler((uint)VsTaskRunContext.UIThreadNormalPriority);
            return Task.Factory.StartNew(action, default(CancellationToken), TaskCreationOptions.HideScheduler, scheduler);
        }

        class AwaitableWrapper : IAwaitable
        {
            Func<IAwaiter> getAwaiter;

            public AwaitableWrapper()
            {
                getAwaiter = () => new AwaiterWrapper();
            }

            public AwaitableWrapper(MainThreadAwaitable awaitable)
            {
                getAwaiter = () => new AwaiterWrapper(awaitable.GetAwaiter());
            }

            public AwaitableWrapper(TaskScheduler scheduler)
            {
                getAwaiter = () => new AwaiterWrapper(new TaskSchedulerAwaiter(scheduler));
            }

            public IAwaiter GetAwaiter() => getAwaiter();
        }

        class AwaiterWrapper : IAwaiter
        {
            Func<bool> isCompleted;
            Action<Action> onCompleted;
            Action getResult;

            public AwaiterWrapper()
            {
                isCompleted = () => true;
                onCompleted = c => c();
                getResult = () => { };
            }

            public AwaiterWrapper(MainThreadAwaiter awaiter)
            {
                isCompleted = () => awaiter.IsCompleted;
                onCompleted = c => awaiter.OnCompleted(c);
                getResult = () => awaiter.GetResult();
            }

            public AwaiterWrapper(TaskSchedulerAwaiter awaiter)
            {
                isCompleted = () => awaiter.IsCompleted;
                onCompleted = c => awaiter.OnCompleted(c);
                getResult = () => awaiter.GetResult();
            }

            public bool IsCompleted => isCompleted();

            public void OnCompleted(Action continuation) => onCompleted(continuation);

            public void GetResult() => getResult();
        }
    }
}
