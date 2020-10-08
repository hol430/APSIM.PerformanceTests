using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace APSIM.PerformanceTests.Models
{
    public class PORename
    {
        public string FileName { get; set; }
        public string TableName { get; set; }
        public string NewTableName { get; set; }

        public string VariableName { get; set; }
        public string NewVariableName { get; set; }

        public string SubmitUser { get; set; }
        public string Type { get; set; }
    }
}