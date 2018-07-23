using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public enum StatusStateEnum
    {
        Expected,
        Error,
        Failure,
        Pending,
        Success,
    }

    public class StatusModel
    {
        public StatusStateEnum State { get; set; }

        public string Context { get; set; }

        public string TargetUrl { get; set; }

        public string Description { get; set; }

        public string AvatarUrl { get; set; }
    }

    public class CheckSuiteModel
    {
        public CheckSuiteConclusionStateEnum? Conclusion { get; set; }

        public CheckSuiteStatusStateEnum Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public List<CheckRunModel> CheckRuns { get; set; }
    }
}