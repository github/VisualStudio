using Microsoft.VisualStudio.Text;
using NSubstitute;

internal static class Args
{
    public static int Int32 { get { return Arg.Any<int>(); } }
    public static string String { get { return Arg.Any<string>(); } }
    public static Span Span { get { return Arg.Any<Span>(); } }
    public static SnapshotPoint SnapshotPoint { get { return Arg.Any<SnapshotPoint>(); } }
}
