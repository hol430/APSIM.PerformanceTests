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
        public Nullable<bool> IsReleased { get; set; }
    }
}