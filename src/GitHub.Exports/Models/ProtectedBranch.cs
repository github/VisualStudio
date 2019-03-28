namespace GitHub.Models
{
    public class ProtectedBranch
    {
        public string Name { get; set; }
        public string[] RequiredStatusCheckContexts { get; set; }
    }
}