using System;
using System.ComponentModel.DataAnnotations;



namespace APSIM.PerformanceTests.Portal.Models
{
    public class ApsimFile
    {
        [Key]
        public int ID { get; set; }

        public int PullRequestId { get; set; }
        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public System.DateTime RunDate { get; set; }
        public Nullable<bool> IsReleased { get; set; }
    }

}