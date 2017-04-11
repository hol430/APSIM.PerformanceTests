using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace APSIM.PerformanceTests.Portal.ModelViews
{
    public class PullRequest
    {
        //this is purely for use im the grid - need to have a string value (not an int)
        public string strPullRequestId { get; set; }

        public int PullRequestId { get; set; }
        public System.DateTime RunDate { get; set; }
        public Nullable<bool> IsReleased { get; set; }
    }
}