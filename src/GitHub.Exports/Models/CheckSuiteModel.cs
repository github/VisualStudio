using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class LastCommitModel
    {
        public string Id { get; set; }
        public List<CheckSuiteModel> CheckSuites { get; set; }

        public List<StatusModel> Statuses { get; set; }
    }

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
    }

    public class CheckSuiteModel
    {
        public CheckConclusionStateEnum? Conclusion { get; set; }

        public CheckStatusStateEnum Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public List<CheckRunModel> CheckRuns { get; set; }
    }
}