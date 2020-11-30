using System;
namespace APSIM.POStats.Migrate.OldModels
{
    public class PredictedObservedTests
    {
        public int ID { get; set; }

        public int PredictedObservedDetailsID { get; set; }

        public string Variable { get; set; }
        public string Test { get; set; }
        public Nullable<double> Accepted { get; set; }
        public Nullable<double> Current { get; set; }
        public Nullable<double> Difference { get; set; }

        public Nullable<double> DifferencePercent { get; set; }

        public Nullable<bool> PassedTest { get; set; }

        //[ForeignKey("PredictedObservedTest.ID")]
        public Nullable<int> AcceptedPredictedObservedTestsID { get; set; }

        public Nullable<bool> IsImprovement { get; set; }

        public int SortOrder { get; set; }

        //public Nullable<bool> IsImprovement { get; set; }

        //public virtual PredictedObservedDetail PredictedObservedDetail { get; set; }
    }
}