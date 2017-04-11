using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace APSIM.PerformanceTests.Portal.ModelViews
{
    public class POTest
    {
        public int ID { get; set; }
        public string strID { get; set; }

        public string Variable { get; set; }
        public string Test { get; set; }
        public double Accepted { get; set; }
        public double Current { get; set; }
        public double Difference { get; set; }
        public bool PassedTest { get; set; }

        public int PredictedObservedDetailsID { get; set; }

    }
}