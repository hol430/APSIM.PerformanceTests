using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace APSIM.PerformanceTests.Portal.Models
{
    public class ApsimFile
    {
        public ApsimFile()
        {
            this.PredictedObservedDetails = new HashSet<PredictedObservedDetail>();
            this.Simulations = new HashSet<Simulation>();
        }

        [Key]
        public int ID { get; set; }

        public int PullRequestId { get; set; }
        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public System.DateTime RunDate { get; set; }
        public Nullable<bool> IsReleased { get; set; }

        public virtual ICollection<PredictedObservedDetail> PredictedObservedDetails { get; set; }
        public virtual ICollection<Simulation> Simulations { get; set; }
    }

}