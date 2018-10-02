using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Splat;

[SetUpFixture]
public class SplatModeDetectorSetUp
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Splat.ModeDetector.OverrideModeDetector(new TrueModeDetector());
    }

    private class TrueModeDetector : IModeDetector
    {
        public bool? InDesignMode() => false;
        public bool? InUnitTestRunner() => true;
    }
}