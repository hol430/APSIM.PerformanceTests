using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace APSIM.PerformanceTests.Portal.Models
{
    public class PredictedObservedDetail
    {
        [Key]
        public int ID { get; set; }

        //[ForeignKey("ApsimFiles.ID")]
        public int ApsimFilesID { get; set; }

        public string TableName { get; set; }
        public string PredictedTableName { get; set; }
        public string ObservedTableName { get; set; }
        public string FieldNameUsedForMatch { get; set; }
        public string FieldName2UsedForMatch { get; set; }
        public string FieldName3UsedForMatch { get; set; }
        public float PassedTests { get; set; }

        public virtual ApsimFile ApsimFile { get; set; }
        //public virtual ICollection<PredictedObservedValue> PredictedObservedValues { get; set; }
    }
}