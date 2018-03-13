using System;
using System.ComponentModel.Composition;
using Task = System.Threading.Tasks.Task;
using GitHub.Exports;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using GitHub.Models;
using System.Linq.Expressions;

namespace GitHub.Services
{
    [ExportForProcess(typeof(IUsageTracker), "devenv")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class UsageTrackerDispatcher : IUsageTracker
    {
        readonly IUsageTracker inner;

        [ImportingConstructor]
        public UsageTrackerDispatcher([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            inner = serviceProvider.GetService(typeof(IUsageTracker)) as IUsageTracker;
        }

        public Task IncrementCounter(Expression<Func<UsageModel.MeasuresModel, int>> counter) => inner.IncrementCounter(counter);
    }
}
