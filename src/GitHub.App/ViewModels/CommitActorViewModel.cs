using GitHub.Models;

namespace GitHub.ViewModels
{
    public class CommitActorViewModel: ActorViewModel, ICommitActorViewModel
    {
        public CommitActorViewModel(CommitActorModel model)
            :base(model.User)
        {
            Name = model.Name;
            Email = model.Email;
            HasLogin = model.User != null;
        }

        public string Email { get; }
        public string Name { get; }
        public bool HasLogin { get; }
    }
}