using GitHub.Extensions;

namespace GitHub.Models
{
    public class InlineAnnotationModel: IInlineAnnotationModel
    {
        readonly CheckSuiteModel checkSuite;
        readonly CheckRunModel checkRun;
        readonly CheckRunAnnotationModel annotation;

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

        public string FileName => annotation.Filename;

        public int StartLine => annotation.StartLine;

        public int EndLine => annotation.EndLine;

        public string CheckRunName => checkRun.Name;

        public string Title => annotation.Title;

        public CheckAnnotationLevel AnnotationLevel => annotation.AnnotationLevel.Value;

        public string Message => annotation.Message;

        public string HeadSha => checkSuite.HeadSha;

        public string LineDescription => $"{StartLine}:{EndLine}";
    }
}