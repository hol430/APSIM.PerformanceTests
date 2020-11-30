using System;
using System.Collections.Generic;

namespace APSIM.POStats.Migrate.OldModels
{
    public class ApsimFiles
    {
        //[Key]
        public int ID { get; set; }
        public int PullRequestId { get; set; }
        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public DateTime RunDate { get; set; }
        public bool StatsAccepted { get; set; }
        public bool IsMerged { get; set; }
        public string SubmitDetails { get; set; }
        public int AcceptedPullRequestId { get; set; }
        public Nullable<DateTime> AcceptedRunDate { get; set; }

        //public DataTable Simulations { get; set; }

        public virtual List<PredictedObservedDetails> PredictedObservedDetails { get; set; }
       
    }
}