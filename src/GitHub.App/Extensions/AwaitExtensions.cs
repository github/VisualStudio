using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub.Extensions
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    public static class AwaitExtensions
    {
        public static TaskSchedulerAwaiter GetAwaiter(this TaskScheduler scheduler)
        {
            Guard.ArgumentNotNull(scheduler, nameof(scheduler));
            return new TaskSchedulerAwaiter(scheduler);
        }

        public struct TaskSchedulerAwaiter : INotifyCompletion
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

            public TaskSchedulerAwaiter(TaskScheduler scheduler)
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