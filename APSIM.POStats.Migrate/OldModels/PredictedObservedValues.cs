using System;
namespace APSIM.POStats.Migrate.OldModels
{
    public class PredictedObservedValues
    {
        public int ID { get; set; }

        public int PredictedObservedDetailsID { get; set; }

        //[ForeignKey("Simulations.ID")]
        //public int SimulationsID { get; set; }

        public string MatchName { get; set; }
        public string MatchValue { get; set; }
        public string MatchName2 { get; set; }
        public string MatchValue2 { get; set; }
        public string MatchName3 { get; set; }
        public string MatchValue3 { get; set; }
        public string ValueName { get; set; }
        public Nullable<double> PredictedValue { get; set; }
        public Nullable<double> ObservedValue { get; set; }

        public int SimulationsID { get; set; }

        public virtual PredictedObservedDetails PredictedObservedDetails { get; set; }
        public virtual Simulations Simulations { get; set; }
    }
}