using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Portal.Models
{
    public class vPredictedObservedTestsFormatted
    {
        public string Variable { get; set; }
        public string Test { get; set; }
        public double? Current { get; set; }
        public string CurrentF { get; set; }
        public double? Accepted { get; set; }
        public string AcceptedF { get; set; }
        public bool? IsImprovement { get; set; }
        public bool? PassedTest { get; set; }

    }
}