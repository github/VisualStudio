using System;
using Xunit;
using Xunit.Sdk;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("STAFactDiscoverer", "UnitTests")]
public class STAFactAttribute : FactAttribute
{
}