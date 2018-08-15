using GitHub.Models;

namespace GitHub.InlineReviews.Services
{
    public class InlineAnnotationModel: IInlineAnnotationModel
    {
        public InlineAnnotationModel(CheckRunModel checkRun, CheckRunAnnotationModel annotation)
        {
            CheckRun = checkRun;
            Annotation = annotation;
        }

        public CheckRunModel CheckRun { get; }
        public CheckRunAnnotationModel Annotation { get; }
    }
}