using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using APSIM.PerformanceTests.Portal.Models;


namespace APSIM.PerformanceTests.Portal.ModelViews
{
    public class ApsimIndex
    {
        public int? PR_Id { get; set; }
        public int? PO_Id { get; set; }

        public IEnumerable<PullRequest> PullRequests { get; set; }
        public IEnumerable<SimFile> SimFiles { get; set; }
    }
}