using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using APSIM.PerformanceTests.Portal.DataAccessLayer;
using APSIM.PerformanceTests.Portal.ViewModel;


namespace APSIM.PerformanceTests.Portal.Controllers
{
    public class HomeController : Controller
    {
        private ApsimDBContext db = new ApsimDBContext();


        public ActionResult Index(int? pullRequestId)
        {
            var vm = new PullRequestViewModel();

            //GET the Pull request Ids
            //var pullRequests = db.ApsimFiles.Select(h => new SelectListItem
            //{
            //    Text = h.PullRequestId.ToString() + "  run on " + h.RunDate.ToString(),
            //    Value = h.PullRequestId.ToString()
            //})
            //.Distinct();
            //vm.PullRequestList = new SelectList(pullRequests, "Value", "Text", null);


            //This loads the top grid of distinct pull request Id, along with the run date and is Released flag
            var pullRequestdetailslist = db.ApsimFiles.Select(h => new PullRequestDetail
            {
                strId = h.PullRequestId.ToString(),
                PullRequestId = h.PullRequestId,
                RunDate = h.RunDate,
                IsReleased = h.IsReleased
            })
            .Distinct()
            .OrderByDescending(h => h.PullRequestId);

            vm.PullRequestDetails = pullRequestdetailslist.ToList();


            //This loads the second grid with the details required
            if (pullRequestId != null)
            {
                ViewBag.PullRequestId = pullRequestId.Value;
                vm.ApsimFiles = db.ApsimFiles
                    .Where(d => d.PullRequestId == pullRequestId)
                    .OrderBy(d => d.PullRequestId)
                    .ThenBy(d => d.FileName)
                    .ToList();
            }

            return View(vm);
        }
    }
}