namespace GitHub.VisualStudio
{
    public interface IActiveDocumentSnapshot
    {
        string Name { get; }
        int StartLine { get; }
        int EndLine { get; }
    }
}
