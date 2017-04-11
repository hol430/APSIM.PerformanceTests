using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using APSIM.PerformanceTests.Portal.Models;


namespace APSIM.PerformanceTests.Portal.ModelViews
{
    public class TestsIndex
    {
        public int? PO_Id { get; set; }
        public string PO_TableName { get; set; }

        public IEnumerable<POTest> POTests { get; set; }


    }
}