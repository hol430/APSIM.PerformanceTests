using System;
using System.Collections.Generic;

namespace APSIM.POStats.Shared.Models
{
    public class PullRequest
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Author { get; set; }
        public DateTime DateRun { get; set; }
        public int? AcceptedPullRequestID { get; set; }
        public virtual List<ApsimFile> Files { get; set; }
        public virtual PullRequest AcceptedPullRequest { get; set; }
    }
}