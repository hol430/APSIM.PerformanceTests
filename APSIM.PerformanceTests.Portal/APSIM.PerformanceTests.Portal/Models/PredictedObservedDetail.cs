using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Portal.Models
{
    public class PredictedObservedDetail
    {
        public PredictedObservedDetail()
        {
            this.PredictedObservedValues = new HashSet<PredictedObservedValue>();
        }

        public int ID { get; set; }
        public int ApsimFilesID { get; set; }
        public string TableName { get; set; }
        public string PredictedTableName { get; set; }
        public string ObservedTableName { get; set; }
        public string FieldNameUsedForMatch { get; set; }
        public string FieldName2UsedForMatch { get; set; }
        public string FieldName3UsedForMatch { get; set; }

        public virtual ApsimFile ApsimFile { get; set; }
        public virtual ICollection<PredictedObservedValue> PredictedObservedValues { get; set; }

    }
}