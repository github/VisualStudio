using System;

namespace GitHub.Models
{
    public interface IPullRequestDetailModel : IPullRequestModel
    {
        string Body { get; }
    }
}
