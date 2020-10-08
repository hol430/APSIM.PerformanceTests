using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Portal.Models
{
    public class vApsimFile
    {
        public int PullRequestId { get; set; }
        public System.DateTime RunDate { get; set; }
        public Nullable<bool> StatsAccepted { get; set; }
        public string SubmitDetails { get; set; }
        public Nullable<double> PercentPassed { get; set; }
        public Nullable<double> Total { get; set; }
        public Nullable<int> AcceptedPullRequestId { get; set; }
        public Nullable<System.DateTime> AcceptedRunDate { get; set; }

    }
}