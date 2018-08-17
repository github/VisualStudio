using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// A tag which marks a line where inline annotations are present
    /// </summary>
    public class ShowInlineAnnotationTag : InlineTagBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInlineCommentTag"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="annotation">A model holding the details of the thread.</param>
        public ShowInlineAnnotationTag(
            IPullRequestSession session,
            IInlineAnnotationModel annotation)
            : base(session, annotation.EndLine)
        {
            Guard.ArgumentNotNull(annotation, nameof(annotation));

            Annotation = annotation;
        }

        /// <summary>
        /// Gets a model holding details of the thread at the tagged line.
        /// </summary>
        public IInlineAnnotationModel Annotation { get; }
    }
}