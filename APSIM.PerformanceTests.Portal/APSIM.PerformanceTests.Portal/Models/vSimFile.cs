using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace APSIM.PerformanceTests.Portal.Models
{
    public class vSimFile
    {
        public int PullRequestId { get; set; }
        public int ApsimFilesID { get; set; }

        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public System.DateTime RunDate { get; set; }
        public Nullable<bool> StatsAccepted { get; set; }
        public string SubmitDetails { get; set; }

        public int PredictedObservedID { get; set; }
        //this is purely for use im the grid - need to have a string value (not an int)
        public string strPredictedObservedID { get; set; }

        public string PredictedObservedTableName { get; set; }
        public Nullable<double> PassedTests { get; set; }

        public Nullable<int>AcceptedPredictedObservedDetailsID { get; set; }

    }
}