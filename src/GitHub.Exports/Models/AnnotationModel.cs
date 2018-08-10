namespace GitHub.Models
{
    public class CheckRunAnnotationModel
    {
        public string BlobUrl { get; set; }

        public int StartLine { get; set; }

        public int EndLine { get; set; }

        public string Filename { get; set; }

        public string Message { get; set; }

        public string Title { get; set; }

        public CheckAnnotationLevel? WarningLevel { get; set; }

        public string RawDetails { get; set; }
    }
}