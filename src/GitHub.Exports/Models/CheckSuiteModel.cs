using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class CheckSuiteModel
    {
        public CheckSuiteConclusionState? Conclusion { get; set; }

        public CheckSuiteStatusState Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public List<CheckRunModel> CheckRuns { get; set; }
    }
}