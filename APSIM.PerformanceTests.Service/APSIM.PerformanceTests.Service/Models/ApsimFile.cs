using System;
using System.Collections.Generic;
using System.Data;

namespace APSIM.PerformanceTests.Models
{
    public class ApsimFile
    {
        public int ID { get; set; }
        public int PullRequestId { get; set; }
        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public DateTime RunDate { get; set; }
        public bool IsReleased { get; set; }

        public DataTable Simulations { get; set; }

        public IEnumerable<PredictedObservedDetails> PredictedObserved { get; set; }

    }

    public class PredictedObservedDetails
    {
        public int ApsimID { get; set; }
        public string DatabaseTableName { get; set; }
        public string PredictedTableName { get; set; }
        public string ObservedTableName { get; set; }
        public string FieldNameUsedForMatch { get; set; }
        public string FieldName2UsedForMatch { get; set; }
        public string FieldName3UsedForMatch { get; set; }

        public DataTable PredictedObservedData { get; set; }
        public ApsimFile apsimFile { get; set; }

    }

}