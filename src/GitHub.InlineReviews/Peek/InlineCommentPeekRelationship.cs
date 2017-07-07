using System;
using Microsoft.VisualStudio.Language.Intellisense;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekRelationship : IPeekRelationship
    {
        static InlineCommentPeekRelationship instance;

        private InlineCommentPeekRelationship()
        {
        }

        public static InlineCommentPeekRelationship Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InlineCommentPeekRelationship();
                }

                return instance;
            }
        }

        public string DisplayName => "GitHub Code Review";
        public string Name => "GitHubCodeReview";
    }
}
