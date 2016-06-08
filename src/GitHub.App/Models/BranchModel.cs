namespace GitHub.Models
{
    public class BranchModel : IBranch
    {
        public BranchModel()
        { }

        public BranchModel(Octokit.Branch branch)
        {
            Name = branch.Name;
        }

        public BranchModel(LibGit2Sharp.Branch branch)
        {
            Name = branch.FriendlyName;
        }

        public string Name { get; set; }
    }
}
