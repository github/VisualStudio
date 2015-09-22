using GitHub.Services;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media.Imaging;

namespace GitHub.Models
{
    public class PullRequestModel : IPullRequestModel
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public int CommentCount { get; set; }
        public bool HasNewComments { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public IAccount Author { get; set; }
    }
}
