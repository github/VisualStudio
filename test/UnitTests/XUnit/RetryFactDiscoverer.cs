using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

public class RetryFactDiscoverer : IXunitTestCaseDiscoverer
{
    readonly IMessageSink diagnosticMessageSink;

    public RetryFactDiscoverer(IMessageSink diagnosticMessageSink)
    {
        this.diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        var maxRetries = factAttribute.GetNamedArgument<int>("MaxRetries");
        if (maxRetries < 1)
            maxRetries = 3;

        yield return new RetryTestCase(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, maxRetries);
    }
}

