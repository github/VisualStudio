using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using System.ComponentModel.Composition;

namespace GitHub.Services
{
    [Export(typeof(IMetricsService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NullMetricsService : IMetricsService
    {
        public Task PostUsage(UsageModel model)
        {
            return Task.CompletedTask;
        }

        public Task SendOptIn()
        {
            return Task.CompletedTask;
        }

        public Task SendOptOut()
        {
            return Task.CompletedTask;
        }
    }
}
