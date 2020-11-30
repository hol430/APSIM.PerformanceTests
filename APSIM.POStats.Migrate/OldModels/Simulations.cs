using System;
using System.ComponentModel.DataAnnotations;


namespace APSIM.POStats.Migrate.OldModels
{
    public class Simulations
    {
        public int ID { get; set; }

        public int ApsimFilesID { get; set; }

        public string Name { get; set; }
        public int OriginalSimulationID { get; set; }

        public virtual ApsimFiles ApsimFiles { get; set; }
    }
}