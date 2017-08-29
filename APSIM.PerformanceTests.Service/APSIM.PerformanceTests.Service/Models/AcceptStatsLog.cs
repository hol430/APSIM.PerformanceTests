using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Models
{
    public class AcceptStatsLog
    {
        public int PullRequestId { get; set; }
        public string SubmitPerson { get; set; }
        public System.DateTime SubmitDate { get; set; }
        public string LogPerson { get; set; }
        public string LogReason { get; set; }
        public bool LogStatus { get; set; }
    }
}