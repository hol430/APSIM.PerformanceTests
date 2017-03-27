using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using APSIM.PerformanceTests.Portal.Models;


namespace APSIM.PerformanceTests.Portal.ViewModel
{
    public class PullRequestViewModel
    {
        [Display(Name = "Selected Pull Request Id")]
        public int? SelectedPullRequestId { get; set; }

        public IEnumerable<SelectListItem> PullRequestList { get; set; }

        public List<ApsimFile> ApsimFiles { get; set; }
        public List<PullRequestDetail> PullRequestDetails { get; set; }

    }
}
