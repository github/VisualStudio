using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GitHub.Models;
using Microsoft.VisualStudio.Telemetry;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    /// <summary>
    /// Implementation of <see cref="IUsageTracker" /> that uses the built in Visual Studio telemetry.
    /// </summary>
    /// <remarks>
    /// This should only be created on Visual Studio 2017 and above.
    /// </remarks>
    public sealed class VisualStudioUsageTracker : IUsageTracker
    {
        const int TelemetryVersion = 1; // Please update the version every time you want to indicate a change in telemetry logic when the extension itself is updated

        const string EventNamePrefix = "vs/github/usagetracker/";
        const string PropertyPrefix = "vs.github.";

        readonly Lazy<IConnectionManager> connectionManager;

        public VisualStudioUsageTracker(Lazy<IConnectionManager> connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        public Task IncrementCounter(Expression<Func<UsageModel.MeasuresModel, int>> counter)
        {
            var property = (MemberExpression)counter.Body;
            var propertyInfo = (PropertyInfo)property.Member;
            var counterName = propertyInfo.Name;
            LogTelemetryEvent(counterName);

            return Task.CompletedTask;
        }

        void LogTelemetryEvent(string counterName)
        {
            const string numberOfPrefix = "numberof";
            if (counterName.StartsWith(numberOfPrefix, StringComparison.OrdinalIgnoreCase))
            {
                counterName = counterName.Substring(numberOfPrefix.Length);
            }

            var operation = new TelemetryEvent(Event.UsageTracker);
            operation.Properties[Property.TelemetryVersion] = TelemetryVersion;
            operation.Properties[Property.CounterName] = counterName;
            operation.Properties[Property.ExtensionVersion] = ExtensionInformation.Version;
            operation.Properties[Property.IsGitHubUser] = IsGitHubUser;
            operation.Properties[Property.IsEnterpriseUser] = IsEnterpriseUser;

            TelemetryService.DefaultSession.PostEvent(operation);
        }

        bool IsEnterpriseUser =>
            connectionManager.Value?.Connections.Any(x => !x.HostAddress.IsGitHubDotCom()) ?? false;

        bool IsGitHubUser =>
            connectionManager.Value?.Connections.Any(x => x.HostAddress.IsGitHubDotCom()) ?? false;

        static class Event
        {
            public const string UsageTracker = EventNamePrefix + "increment-counter";
        }

        static class Property
        {
            public const string TelemetryVersion = PropertyPrefix + nameof(TelemetryVersion);
            public const string CounterName = PropertyPrefix + nameof(CounterName);
            public const string ExtensionVersion = PropertyPrefix + nameof(ExtensionVersion);
            public const string IsGitHubUser = PropertyPrefix + nameof(IsGitHubUser);
            public const string IsEnterpriseUser = PropertyPrefix + nameof(IsEnterpriseUser);
        }
    }
}
