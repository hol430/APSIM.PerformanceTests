using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Portal.Models
{
    public class vSimulationPredictedObserved
    {
        public int ID { get; set; }
        public string MatchNames { get; set; }
        public string MatchValues { get; set; }
        public string ValueName { get; set; }

        public Nullable<double> CurrentPredictedValue { get; set; }
        public Nullable<double> CurrentObservedValue { get; set; }

        public Nullable<double> AcceptedPredictedValue { get; set; }
        public Nullable<double> AcceptedObservedValue { get; set; }

        public int SimulationsID { get; set; }
        public string SimulationName { get; set; }

    }
}