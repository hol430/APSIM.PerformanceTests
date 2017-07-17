using System;
using System.ComponentModel.DataAnnotations;


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
        public Nullable<double> PassedTests { get; set; }

        //public virtual ApsimFile ApsimFile { get; set; }
        //public virtual ICollection<PredictedObservedValue> PredictedObservedValues { get; set; }
    }
}