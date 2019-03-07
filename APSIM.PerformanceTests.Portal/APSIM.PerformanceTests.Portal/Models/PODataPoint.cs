using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Portal.Models
{
    public struct PODataPoint
    {
        public double X { get; set; }

        public double Y { get; set; }

        public string SimulationName { get; set; }
    }
}