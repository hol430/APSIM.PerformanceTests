using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace APSIM.PerformanceTests.Portal.Models
{
    public class Simulation
    {
        [Key]
        public int ID { get; set; }

        //[ForeignKey("ApsimFile")]
        public int ApsimFilesID { get; set; }

        public string Name { get; set; }
        public int OriginalSimulationID { get; set; }

        public virtual ApsimFile ApsimFiles { get; set; }
    }
}