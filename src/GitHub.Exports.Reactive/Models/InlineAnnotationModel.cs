using GitHub.Extensions;

namespace GitHub.Models
{
    /// <summary>
    /// Represents an inline annotation on an <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    public class InlineAnnotationModel
    {
        private CheckRunModel checkRun;
        private CheckRunAnnotationModel annotation;

        /// <summary>
        /// Initializes the <see cref="InlineAnnotationModel"/>.
        /// </summary>
        /// <param name="checkRun">The check run model.</param>
        /// <param name="annotation">The annotation model.</param>
        public InlineAnnotationModel(CheckRunModel checkRun, CheckRunAnnotationModel annotation)
        {
            Guard.ArgumentNotNull(annotation.AnnotationLevel, nameof(annotation.AnnotationLevel));

            this.checkRun = checkRun;
            this.annotation = annotation;
        }
        /// Gets the start line of the annotation.
        /// </summary>
        public int StartLine => annotation.StartLine;

        /// <summary>
        /// Gets the end line of the annotation.
        /// </summary>
        public int EndLine => annotation.EndLine;
        
        /// <summary>
        /// Gets the annotation level.
        /// </summary>
        public CheckAnnotationLevel AnnotationLevel => annotation.AnnotationLevel;
    }
}