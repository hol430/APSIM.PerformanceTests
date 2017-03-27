using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Portal.ViewModel
{
    public class PullRequestDetail
    {
        public string strId { get; set; }
        public int PullRequestId { get; set; }
        public System.DateTime RunDate { get; set; }
        public Nullable<bool> IsReleased { get; set; }
    }
}