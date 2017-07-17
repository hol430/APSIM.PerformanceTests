using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using APSIM.PerformanceTests.Portal.Models;

public class ApsimFilesDS
{
    //NOTE:  Dont forget that these need to have the build property set to compile

    public static ApsimFile GetByID(int id)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.ApsimFiles
                .Where(a => a.ID == id)
                .SingleOrDefault();
        }
    }

    public static ApsimFile GetByPredictedObservedId(int predictedObservedId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return (from pod in context.PredictedObservedDetails
                   join af in context.ApsimFiles on pod.ApsimFilesID equals af.ID
                   where pod.ID == predictedObservedId
                   select af)
                .SingleOrDefault();
        }
    }

    public static int GetLatestReleasedPullRequestId(int currentPullRequestId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.ApsimFiles
                .Where(a => a.IsReleased == true && a.PullRequestId != currentPullRequestId)
                .OrderByDescending(a => a.RunDate)
                .Select(a => a.PullRequestId)
                .First();
        }
    }


    public static List<vApsimFile> GetAllApsimFiles()
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.ApsimFiles
                .Select(h => new vApsimFile
                {
                    PullRequestId = h.PullRequestId,
                    RunDate = h.RunDate,
                    IsReleased = h.IsReleased
                })
                .Distinct()
                .OrderByDescending(h => h.RunDate)
                .ThenByDescending(h => h.PullRequestId)
                .ToList();
        }
    }

    public static List<vSimFile> GetSimFilesByPullRequestIDandDate(int pullRequestId, DateTime runDate)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {

            string pathStr = @"C:\Jenkins\workspace\1. GitHub pull request\ApsimX\Tests\Validation\Maize\Maize.apsimx";
            int posn = pathStr.IndexOf(@"ApsimX\Tests");
            posn += 7;

            return context.ApsimFiles.Join(context.PredictedObservedDetails,
                    af => af.ID,
                    pod => pod.ApsimFilesID,
                    (af, pod) => new { ApsimFiles = af, PredictedObservedDetails = pod })
                    .Where(sf => sf.ApsimFiles.PullRequestId == pullRequestId && sf.ApsimFiles.RunDate == runDate)
                    .Select(sf => new vSimFile
                    {
                        PullRequestId = sf.ApsimFiles.PullRequestId,
                        FileName = sf.ApsimFiles.FileName,
                        FullFileName = ((sf.ApsimFiles.FullFileName.Contains("GitHub")) ? sf.ApsimFiles.FullFileName.Substring(posn) : sf.ApsimFiles.FullFileName),
                        PredictedObservedID = sf.PredictedObservedDetails.ID,
                        strPredictedObservedID = sf.PredictedObservedDetails.ID.ToString(),
                        PredictedObservedTableName = sf.PredictedObservedDetails.TableName,
                        PassedTests = sf.PredictedObservedDetails.PassedTests,
                    })
                    .OrderBy(sf => sf.FileName)
                    .ToList();
        }
    }

}
