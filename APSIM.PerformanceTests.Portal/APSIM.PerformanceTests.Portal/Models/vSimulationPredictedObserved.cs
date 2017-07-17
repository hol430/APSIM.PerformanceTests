using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Portal.Models
{
    public class vSimulationPredictedObserved
    {
        public string TableName { get; set; }
        public int SimulationsID { get; set; }
        public string SimulationName { get; set; }
        public string MatchNames { get; set; }
        public string MatchValues { get; set; }
        public string ValueName { get; set; }

        public Nullable<double> PredictedValue { get; set; }
        public Nullable<double> ObservedValue { get; set; }
        public Nullable<double> Difference { get; set; }

    }
}