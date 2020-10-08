using System;
using System.ComponentModel.DataAnnotations;


namespace APSIM.PerformanceTests.Portal.Models
{
    public class PredictedObservedValue
    {
        [Key]
        public int ID { get; set; }

        //[ForeignKey("PredictedObservedDetails.ID")]
        public int PredictedObservedDetailsID { get; set; }

        //[ForeignKey("Simulations.ID")]
        public int SimulationsID { get; set; }

        public string MatchName { get; set; }
        public string MatchValue { get; set; }
        public string MatchName2 { get; set; }
        public string MatchValue2 { get; set; }
        public string MatchName3 { get; set; }
        public string MatchValue3 { get; set; }
        public string ValueName { get; set; }
        public Nullable<double> PredictedValue { get; set; }
        public Nullable<double> ObservedValue { get; set; }

        public virtual PredictedObservedDetail PredictedObservedDetail { get; set; }
        public virtual Simulation Simulation { get; set; }
    }
}