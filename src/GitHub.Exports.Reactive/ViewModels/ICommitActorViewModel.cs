namespace GitHub.ViewModels
{
    public interface ICommitActorViewModel : IActorViewModel
    {
        string Email { get; }
        string Name { get; }
        bool HasLogin { get; }
    }
}