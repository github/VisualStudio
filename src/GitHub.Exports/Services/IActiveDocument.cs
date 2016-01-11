namespace GitHub.VisualStudio
{
    public interface IActiveDocument
    {
        string Name { get; }
        int AnchorLine { get; }
        int AnchorColumn { get; }
        int EndLine { get; }
        int EndColumn { get; }
    }
}
