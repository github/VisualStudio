using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.VisualStudio;
using MethodDecoratorInterfaces;

public class TestBaseClass : IEntryExitDecorator
{
    public void OnEntry()
    {
        Splat.ModeDetector.Current.SetInUnitTestRunner(true);
    }

    public void OnExit()
    {
    }
}
