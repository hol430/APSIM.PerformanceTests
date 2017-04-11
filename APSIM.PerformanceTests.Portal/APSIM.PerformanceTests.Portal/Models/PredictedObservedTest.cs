using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


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
        public double Accepted { get; set; }
        public double Current { get; set; }
        public double Difference { get; set; }
        public bool PassedTest { get; set; }

        //public virtual PredictedObservedDetail PredictedObservedDetail { get; set; }
    }
}