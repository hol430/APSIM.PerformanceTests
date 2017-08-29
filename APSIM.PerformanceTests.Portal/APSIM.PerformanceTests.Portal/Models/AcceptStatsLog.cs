using System;
using System.ComponentModel.DataAnnotations;

namespace APSIM.PerformanceTests.Portal.Models
{
    public class AcceptStatsLog
    {
        [Key]
        public int ID { get; set; }

        public int PullRequestId { get; set; }
        public string SubmitPerson { get; set; }
        public System.DateTime SubmitDate { get; set; }
        public string LogPerson { get; set; }
        public string LogReason { get; set; }
        public bool LogStatus { get; set; }


    }
}