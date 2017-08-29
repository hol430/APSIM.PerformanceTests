using System;
using System.ComponentModel.DataAnnotations;


namespace APSIM.PerformanceTests.Portal.Models
{
    public class PredictedObservedTest
    {
        [Key]
        public int ID { get; set; }

        //[ForeignKey("PredictedObservedDetails.ID")]
        public int PredictedObservedDetailsID { get; set; }

        public string Variable { get; set; }
        public string Test { get; set; }
        public Nullable<double> Accepted { get; set; }
        public Nullable<double> Current { get; set; }
        public Nullable<double> Difference { get; set; }
        public Nullable<bool> PassedTest { get; set; }

        //[ForeignKey("PredictedObservedTest.ID")]
        public Nullable<int> AcceptedPredictedObservedTestsID { get; set; }


        //public virtual PredictedObservedDetail PredictedObservedDetail { get; set; }
    }
}