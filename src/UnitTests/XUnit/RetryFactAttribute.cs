using Xunit;
using Xunit.Sdk;

/// <summary>
/// Works just like [Fact] except that failures are retried (by default, 3 times).
/// </summary>
[XunitTestCaseDiscoverer("RetryFactDiscoverer", "GitHub.Tests.Utils")]
public class RetryFactAttribute : FactAttribute
{
    /// <summary>
    /// Number of retries allowed for a failed test. If unset (or set less than 1), will
    /// default to 3 attempts.
    /// </summary>
    public int MaxRetries { get; set; }
}