using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class CheckRunModel
    {
        public CheckSuiteConclusionStateEnum? Conclusion { get; set; }

        public CheckSuiteStatusStateEnum Status { get; set; }

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public List<CheckRunAnnotationModel> Annotations { get; set; }
    }
}