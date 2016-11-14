using System.Threading.Tasks;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using GitHub.Extensions;
using System.Runtime.CompilerServices;
using System;
using static Microsoft.VisualStudio.Threading.JoinableTaskFactory;
using System.Threading;

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
        /// <summary>
        /// Switch to the UI thread using ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync
        /// Auto-disables switching when running in unit test mode
        /// </summary>
        /// <returns></returns>
        public static IAwaitable SwitchToMainThreadAsync()
        {
            return Guard.InUnitTestRunner() ?
                new AwaitableWrapper() :
                new AwaitableWrapper(ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync());
        }

        /// <summary>
        /// Switch to a thread pool background thread if the current thread isn't one, otherwise does nothing
        /// Auto-disables switching when running in unit test mode
        /// </summary>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static IAwaitable SwitchToPoolThreadAsync(TaskScheduler scheduler)
        {
            return Guard.InUnitTestRunner() ?
                new AwaitableWrapper() :
                new AwaitableWrapper(scheduler);
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
                getAwaiter = () => new AwaiterWrapper(new BackgroundThreadAwaiter(scheduler));
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
                getResult = () => {};
            }

            public AwaiterWrapper(MainThreadAwaiter awaiter)
            {
                isCompleted = () => awaiter.IsCompleted;
                onCompleted = c => awaiter.OnCompleted(c);
                getResult = () => awaiter.GetResult();
            }

            public AwaiterWrapper(BackgroundThreadAwaiter awaiter)
            {
                isCompleted = () => awaiter.IsCompleted;
                onCompleted = c => awaiter.OnCompleted(c);
                getResult = () => awaiter.GetResult();
            }

            public bool IsCompleted => isCompleted();

            public void OnCompleted(Action continuation) => onCompleted(continuation);

            public void GetResult() => getResult();
        }

        struct BackgroundThreadAwaiter : INotifyCompletion
        {
            readonly TaskScheduler scheduler;

            public bool IsCompleted
            {
                get
                {
                    bool isThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                    return (scheduler == TaskScheduler.Default & isThreadPoolThread) || (scheduler == TaskScheduler.Current && TaskScheduler.Current != TaskScheduler.Default);
                }
            }

            public BackgroundThreadAwaiter(TaskScheduler scheduler)
            {
                Guard.ArgumentNotNull(scheduler, nameof(scheduler));
                this.scheduler = scheduler;
            }

            public void OnCompleted(Action continuation)
            {
                Task.Factory.StartNew(continuation, CancellationToken.None, TaskCreationOptions.None, scheduler);
            }

            public void GetResult()
            {
            }
        }
    }
}
