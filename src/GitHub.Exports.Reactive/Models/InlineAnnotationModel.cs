using GitHub.Extensions;

namespace GitHub.Models
{
    public class InlineAnnotationModel: IInlineAnnotationModel
    {
        private CheckRunModel checkRun;
        private CheckRunAnnotationModel annotation;

        public InlineAnnotationModel(CheckRunModel checkRun, CheckRunAnnotationModel annotation)
        {
            Guard.ArgumentNotNull(annotation.AnnotationLevel, nameof(annotation.AnnotationLevel));

            this.checkRun = checkRun;
            this.annotation = annotation;
        }

        public int StartLine => annotation.StartLine;

        public int EndLine => annotation.EndLine;

        public CheckAnnotationLevel AnnotationLevel => annotation.AnnotationLevel.Value;
    }
}