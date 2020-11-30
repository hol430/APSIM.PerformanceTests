using System;
using System.Collections.Generic;
namespace APSIM.POStats.Migrate.OldModels
{
    public class PredictedObservedDetails
    {
        public int ID { get; set; }

        public int ApsimFilesID { get; set; }

        public string TableName { get; set; }
        public string PredictedTableName { get; set; }
        public string ObservedTableName { get; set; }
        public string FieldNameUsedForMatch { get; set; }
        public string FieldName2UsedForMatch { get; set; }
        public string FieldName3UsedForMatch { get; set; }
        public Nullable<double> PassedTests { get; set; }

        public int HasTests { get; set; }

        public Nullable<int> AcceptedPredictedObservedDetailsID { get; set; }

        //        public DataTable Data { get; set; }
        public virtual ApsimFiles ApsimFiles { get; set; }
        public virtual List<PredictedObservedValues> PredictedObservedValues { get; set; }
    }
}