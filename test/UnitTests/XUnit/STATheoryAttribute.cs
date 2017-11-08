using System;
using Xunit;
using Xunit.Sdk;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("STATheoryDiscoverer", "UnitTests")]
public class STATheoryAttribute : TheoryAttribute
{
}
