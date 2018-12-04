using GitHub.Extensions;

namespace GitHub.Models
{
    /// <summary>
    /// Represents an inline annotation on an <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    public class InlineAnnotationModel
    {
        readonly CheckSuiteModel checkSuite;
        readonly CheckRunModel checkRun;
        readonly CheckRunAnnotationModel annotation;

        /// <summary>
        /// Initializes the <see cref="InlineAnnotationModel"/>.
        /// </summary>
        /// <param name="checkSuite">The check suite model.</param>
        /// <param name="checkRun">The check run model.</param>
        /// <param name="annotation">The annotation model.</param>
        public InlineAnnotationModel(CheckSuiteModel checkSuite, CheckRunModel checkRun,
            CheckRunAnnotationModel annotation)
        {
            Guard.ArgumentNotNull(checkRun, nameof(checkRun));
            Guard.ArgumentNotNull(annotation, nameof(annotation));
            Guard.ArgumentNotNull(annotation.AnnotationLevel, nameof(annotation.AnnotationLevel));

            this.checkSuite = checkSuite;
            this.checkRun = checkRun;
            this.annotation = annotation;
        }

        /// <summary>
        /// Gets the annotation path.
        /// </summary>
        public string Path => annotation.Path;

        /// <summary>
        /// Gets the 1-based start line of the annotation.
        /// </summary>
        public int StartLine => annotation.StartLine;

        /// <summary>
        /// Gets the 1-based end line of the annotation.
        /// </summary>
        public int EndLine => annotation.EndLine;
        
        /// <summary>
        /// Gets the annotation level.
        /// </summary>
        public CheckAnnotationLevel AnnotationLevel => annotation.AnnotationLevel;

        /// <summary>
        /// Gets the name of the check suite.
        /// </summary>
        public string CheckSuiteName => checkSuite.ApplicationName;

        /// <summary>
        /// Gets the name of the check run.
        /// </summary>
        public string CheckRunName => checkRun.Name;

        /// <summary>
        /// Gets the annotation title.
        /// </summary>
        public string Title => annotation.Title;

        /// <summary>
        /// Gets the annotation message.
        /// </summary>
        public string Message => annotation.Message;

        /// <summary>
        /// Gets the sha the check run was created on.
        /// </summary>
        public string HeadSha => checkSuite.HeadSha;

        /// <summary>
        /// Gets the a descriptor for the line(s) reported.
        /// </summary>
        public string LineDescription => $"{StartLine}:{EndLine}";
    }
}