using GitHub.Primitives;

namespace GitHub.Models
{
    public class GitReferenceModel
    {
        public string Ref { get; set; }
        public string Label { get; set; }
        public UriString RepositoryCloneUrl { get; set; }
    }
}
