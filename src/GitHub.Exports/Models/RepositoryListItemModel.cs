using System;

namespace GitHub.Models
{
    public class RepositoryListItemModel
    {
        public bool IsFork { get; set; }
        public bool IsPrivate { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public Uri Url { get; set; }
    }
}
