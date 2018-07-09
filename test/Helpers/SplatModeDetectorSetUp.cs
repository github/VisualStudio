using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

[SetUpFixture]
public class SplatModeDetectorSetUp
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Splat.ModeDetector.Current.SetInUnitTestRunner(true);
    }
}