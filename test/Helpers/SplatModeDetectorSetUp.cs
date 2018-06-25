using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

[SetUpFixture]
public class SplatModeDetectorSetUp
{
    static SplatModeDetectorSetUp()
    {
        // HACK: Force .NET 4.5 version of Splat to load when executing inside NCrunch
        var ncrunchAsms = Environment.GetEnvironmentVariable("NCrunch.AllAssemblyLocations")?.Split(';');
        if (ncrunchAsms != null)
        {
            ncrunchAsms.Where(x => x.EndsWith(@"\Net45\Splat.dll", StringComparison.OrdinalIgnoreCase)).Select(Assembly.LoadFrom).FirstOrDefault();
        }
    }

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Splat.ModeDetector.Current.SetInUnitTestRunner(true);
    }
}