namespace GitHub.ViewModels
{
    public interface IInfoPanel
    {
        string Message { get; set; }
        MessageType MessageType { get; set; }
    }

    public enum MessageType
    {
        Information,
        Warning
    }
}