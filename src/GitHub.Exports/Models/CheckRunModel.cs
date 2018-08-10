using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class CheckRunModel
    {
        public CheckConclusionState? Conclusion { get; set; }

        public CheckStatusState Status { get; set; }

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public List<CheckRunAnnotationModel> Annotations { get; set; }

        public string Name { get; set; }

        public string DetailsUrl { get; set; }

        public string Summary { get; set; }
    }
}