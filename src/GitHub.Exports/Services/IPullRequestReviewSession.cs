using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestReviewSession
    {
        IEnumerable<IPullRequestReviewCommentModel> GetCommentsForFile(string filePath);
    }
}
