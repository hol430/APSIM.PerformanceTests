using System;

namespace APSIM.POStats.Migrate.OldModels
{
    public class AcceptStatsLogs
    {
        public int ID { get; set; }

        public int PullRequestId { get; set; }
        public string SubmitPerson { get; set; }
        public System.DateTime SubmitDate { get; set; }

        public int FileCount { get; set; }

        public string LogPerson { get; set; }
        public string LogReason { get; set; }
        public bool LogStatus { get; set; }
        public System.DateTime LogAcceptDate { get; set; }

        public int StatsPullRequestId { get; set; }

    }
}