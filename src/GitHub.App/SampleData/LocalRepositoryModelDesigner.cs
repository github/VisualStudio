using GitHub.Models;
using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.App.SampleData
{
    public class LocalRepositoryModelDesigner : LocalRepositoryModel
    {
        public new UriString CloneUrl { get; set; }
        public BranchModel CurrentBranch { get; set; }
        public new Octicon Icon { get; set; }
        public new string LocalPath { get; set; }
        public new string Name { get; set; }
        public new string Owner { get; set; }
    }
}
