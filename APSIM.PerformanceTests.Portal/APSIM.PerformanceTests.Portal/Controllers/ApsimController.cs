using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using APSIM.PerformanceTests.Portal.ModelViews;
using APSIM.PerformanceTests.Portal.DataAccessLayer;


namespace APSIM.PerformanceTests.Portal.Controllers
{
    public class ApsimController : Controller
    {
        private ApsimDBContext db = new ApsimDBContext();

        // GET: Apsim
        public ActionResult Index(int? pullRequestId)
        {
            //This loads the top grid of distinct pull request Id, along with the run date and is Released flag
            //SELECT DISTINCT PullRequestId, RunDate, IsReleased
            //FROM   dbo.ApsimFiles
            //ORDER BY PullRequestId DESC

            ApsimIndex apsimViewModel = new ApsimIndex();
            apsimViewModel.PullRequests = db.ApsimFiles.Select(h => new PullRequest
            {
                strPullRequestId = h.PullRequestId.ToString(),
                PullRequestId = h.PullRequestId,
                RunDate = h.RunDate,
                IsReleased = h.IsReleased
            })
            .Distinct()
            .OrderByDescending(h => h.PullRequestId);

            return View(apsimViewModel);
        }



        public ActionResult ShowSimFiles(int? pullRequestId)
        {
            ApsimIndex apsimViewModel = new ApsimIndex();

            //This loads the second grid with the details required
            if (pullRequestId != null)
            {
                ViewBag.PullRequestId = pullRequestId.Value;


                //this is a fudge - need to create an extension helper for this
                string pathStr = @"C:\Jenkins\workspace\1. GitHub pull request\ApsimX\Tests\Validation\Maize\Maize.apsimx";
                int posn = pathStr.IndexOf(@"ApsimX\Tests");
                posn += 7;


                //SELECT a.[PullRequestId], a.[FileName], a.[FullFileName], a.[RunDate], a.[IsReleased], a.[ID] AS ApsimFilesID,
                //       d.[TableName] AS PredictedObservedTableName, d.[PassedTests], d.[ID] AS PredictedObservedID
                //  FROM [dbo].ApsimFiles a INNER JOIN dbo.PredictedObservedDetails d
                //      ON a.[ID] = d.[ApsimFilesID]
                //WHERE a.[PullRequestId] = @PullRequestId
                //ORDER BY a.[FileName],


                // FullFileName = ((sf.ApsimFiles.FullFileName.Length > posn) ? sf.ApsimFiles.FullFileName.Substring(posn) : sf.ApsimFiles.FullFileName),

                apsimViewModel.PR_Id = pullRequestId;

                var updatedSimFiles = db.ApsimFiles.Join(db.PredictedObservedDetails,
                                        af => af.ID,
                                        pod => pod.ApsimFilesID,
                                        (af, pod) => new { ApsimFiles = af, PredictedObservedDetails = pod })
                                        .Where(sf => sf.ApsimFiles.PullRequestId == pullRequestId)
                                        .Select(sf => new SimFile
                                        {
                                            PullRequestId = sf.ApsimFiles.PullRequestId,
                                            FileName = sf.ApsimFiles.FileName,
                                            FullFileName = sf.ApsimFiles.FullFileName,
                                            PredictedObservedID = sf.PredictedObservedDetails.ID,
                                            strPredictedObservedID = sf.PredictedObservedDetails.ID.ToString(),
                                            PredictedObservedTableName = sf.PredictedObservedDetails.TableName,
                                            PassedTests = sf.PredictedObservedDetails.PassedTests,
                                        })
                                        .OrderBy(sf => sf.FileName)
                                        .forEach(f => f.FullFileName;

                apsimViewModel.SimFiles = updatedSimFiles;
            }
            return PartialView("ShowSimFiles", apsimViewModel);
        }

 
    }
}


public static class MyExtentionMethods
{
    public static void UpdateFileName<T>(this IEnumerable<T> fileNames)
    {
        foreach (string item in fileNames)
        {
            int posn = item.IndexOf(@"ApsimX\Tests");
            if (posn <= 0)
            {
                posn = item.IndexOf(@"ApsimX\Prototypes");
            }
            if (posn <= 0)
            {
                posn = item.IndexOf("Prototypes");
            }
            if (posn < 0)
            {
                posn = 0;
            }
            yield return item.Substring(posn);
        }
    }
}
