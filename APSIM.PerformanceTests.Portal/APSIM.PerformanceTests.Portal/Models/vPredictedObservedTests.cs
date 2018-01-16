using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace APSIM.PerformanceTests.Portal.Models
{
    public class vPredictedObservedTests
    {

        public string FileName { get; set; }
        public string TableName { get; set; }
        public string Variable { get; set; }
        public string Test { get; set; }
        public Nullable<double> Accepted { get; set; }
        public Nullable<double> Current { get; set; }
        public Nullable<double> Difference { get; set; }
        public Nullable<double> DifferencePercent { get; set; }
        public Nullable<bool> PassedTest { get; set; }
        public Nullable<bool> IsImprovement { get; set; }
        public int PredictedObservedDetailsID { get; set; }
        [Key]
        public int PredictedObservedTestsID { get; set; }
        public int SortOrder { get; set; }

    }
}