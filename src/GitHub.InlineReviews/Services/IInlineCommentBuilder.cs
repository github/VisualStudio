using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.InlineReviews.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Text;

namespace GitHub.InlineReviews.Services
{
    public interface IInlineCommentBuilder
    {
        Task<IList<InlineCommentModel>> Build(
            string path,
            ITextSnapshot snapshot,
            IPullRequestReviewSession session);
    }
}