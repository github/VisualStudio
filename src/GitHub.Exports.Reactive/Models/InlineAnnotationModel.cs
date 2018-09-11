using GitHub.Extensions;

namespace GitHub.Models
{
    /// <inheritdoc />
    public class InlineAnnotationModel: IInlineAnnotationModel
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

        /// <inheritdoc />
        public int StartLine => annotation.StartLine;

        /// <inheritdoc />
        public int EndLine => annotation.EndLine;

        /// <inheritdoc />
        public CheckAnnotationLevel AnnotationLevel => annotation.AnnotationLevel;
    }
}