namespace GitHub.Models
{
    public class StatusModel
    {
        public StatusState State { get; set; }

        public string Context { get; set; }

        public string TargetUrl { get; set; }

        public string Description { get; set; }

        public string AvatarUrl { get; set; }
    }
}