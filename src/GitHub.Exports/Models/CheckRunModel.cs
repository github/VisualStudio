using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class CheckRunModel
    {
        public CheckConclusionStateEnum? Conclusion { get; set; }

        public CheckStatusStateEnum Status { get; set; }

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public List<CheckRunAnnotationModel> Annotations { get; set; }
    }
}