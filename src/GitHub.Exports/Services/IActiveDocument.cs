namespace GitHub.VisualStudio
{
    public interface IActiveDocument
    {
        string Name { get; }
        int Line { get; }
    }
}
