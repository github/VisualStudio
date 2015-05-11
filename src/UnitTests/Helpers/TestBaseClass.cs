using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntryExitDecoratorInterfaces;
using GitHub.VisualStudio;

/// <summary>
/// This base class will get its methods called by the most-derived
/// classes. The calls are injected by the EntryExitMethodDecorator Fody
/// addin, so don't be surprised if you don't see any calls in the code.
/// </summary>
public class TestBaseClass : IEntryExitDecorator
{
    public void OnEntry()
    {
        // Ensure that every test has the InUnitTestRunner flag
        // set, so threading doesn't go nuts.
        Splat.ModeDetector.Current.SetInUnitTestRunner(true);
    }

    public void OnExit()
    {
    }
}
