using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Portal.Models
{
    public class Simulation
    {
        public Simulation()
        {
            this.PredictedObservedValues = new HashSet<PredictedObservedValue>();
        }

        public int ID { get; set; }
        public int ApsimFilesID { get; set; }
        public string Name { get; set; }
        public int OriginalSimulationID { get; set; }

        public virtual ApsimFile ApsimFile { get; set; }
        public virtual ICollection<PredictedObservedValue> PredictedObservedValues { get; set; }

    }
}