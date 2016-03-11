using System.Diagnostics.CodeAnalysis;
using GitHub.ViewModels;
using System.Collections.Generic;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestCreationViewModelDesigner : BaseViewModelDesigner, IPullRequestCreationViewModel
    {
        public PullRequestCreationViewModelDesigner()
        {
            Name = "Bla bla bla";
            Users = new List<string>()
            {
                "haacked",
                "shana"
            };
        }

        public string Name { get; set; }
        public List<string> Users { get; set; }
    }
}