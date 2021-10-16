using GitHub.VisualStudio;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using GitHub.Models;

namespace GitHub.Services
{
    [Guid(Guids.UsageTrackerId)]
    public interface IUsageTracker
    {
        Task IncrementCounter(Expression<Func<UsageModel.MeasuresModel, int>> counter);
    }
}
