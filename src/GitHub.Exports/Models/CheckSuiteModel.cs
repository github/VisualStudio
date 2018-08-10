using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class CheckSuiteModel
    {
        public CheckConclusionState? Conclusion { get; set; }

        public CheckStatusState Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public List<CheckRunModel> CheckRuns { get; set; }
    }
}