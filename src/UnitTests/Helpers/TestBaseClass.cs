using EntryExitDecoratorInterfaces;
using System.IO;

/// <summary>
/// This base class will get its methods called by the most-derived
/// classes. The calls are injected by the EntryExitMethodDecorator Fody
/// addin, so don't be surprised if you don't see any calls in the code.
/// </summary>
public class TestBaseClass : IEntryExitDecorator
{
    public virtual void OnEntry()
    {
        // Ensure that every test has the InUnitTestRunner flag
        // set, so threading doesn't go nuts.
        Splat.ModeDetector.Current.SetInUnitTestRunner(true);
    }

    public virtual void OnExit()
    {
    }
}

public class TempFileBaseClass : TestBaseClass
{
    public DirectoryInfo Directory { get; set; }

    public override void OnEntry()
    {
        var f = Path.GetTempFileName();
        var name = Path.GetFileNameWithoutExtension(f);
        File.Delete(f);
        Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), name));
        Directory.Create();
        base.OnEntry();
    }

    public override void OnExit()
    {
        Directory.Delete(true);
        base.OnExit();
    }
}