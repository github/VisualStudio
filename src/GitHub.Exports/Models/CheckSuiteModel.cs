using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class CheckSuiteModel
    {
        public CheckSuiteConclusionStateEnum? Conclusion { get; set; }

        public CheckSuiteStatusStateEnum Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public List<CheckRunModel> CheckRuns { get; set; }
    }
}