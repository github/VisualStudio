using System;
using System.Diagnostics;
using GitHub.Logging;
using GitHub.Services;
using Serilog;

namespace GitHub.Extensions
{
    public static class IServiceProviderExtensions
    {
        static readonly ILogger log = LogManager.ForContext<VSServices>();

        /// <summary>
        /// Safe variant of GetService that doesn't throw exceptions if the service is
        /// not found.
        /// </summary>
        /// <returns>The service, or null if not found</returns>
        public static object GetServiceSafe(this IServiceProvider serviceProvider, Type type)
        {
            var ui = serviceProvider as IGitHubServiceProvider;
            if (ui == null)
            {
                try
                {
                    var ret = serviceProvider.GetService(type);
                    if (ret != null)
                        return ret;
                }
                catch { }
            }

            try
            {
                if (ui == null)
                {
                    ui = serviceProvider.GetService(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;
                }
                if (type.IsEquivalentTo(typeof(IGitHubServiceProvider)))
                    return ui;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                log.Error(ex, "GetServiceSafe: Could not obtain instance of '{Type}'", type);
            }
            return ui?.TryGetService(type);
        }

        /// <summary>
        /// Safe generic variant that calls <see cref="TryGetService(IServiceProvider, Type)"/>
        /// so it doesn't throw exceptions if the service is not found
        /// </summary>
        /// <returns>The service, or null if not found</returns>
        public static T GetServiceSafe<T>(this IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.GetServiceSafe(typeof(T)) as T;
        }
    }
}
