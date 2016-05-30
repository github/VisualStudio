using System;
using System.Reactive;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IMetricsService
    {
        /// <summary>
        /// Posts the provided usage model.
        /// </summary>
        IObservable<Unit> PostUsage(UsageModel model);
        
        /// <summary>
        /// Sends an empty request that indicates that the user has chosen to opt out of usage
        /// tracking.
        /// </summary>
        IObservable<Unit> SendOptOut();

        /// <summary>
        /// Sends an empty request that indicates that the user has chosen to opt back in to
        /// usage tracking.
        /// </summary>
        IObservable<Unit> SendOptIn();
    }
}
