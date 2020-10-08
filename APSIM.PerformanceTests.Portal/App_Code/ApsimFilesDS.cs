using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using APSIM.PerformanceTests.Portal.Models;

public class ApsimFilesDS
{
    //NOTE:  Dont forget that these need to have the build property set to compile

    /// <summary>
    /// Returns details of ApsimFile based on the ID (int) of the record
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static ApsimFile GetByID(int id)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.ApsimFiles
                .Where(a => a.ID == id)
                .SingleOrDefault();
        }
    }

    /// <summary>
    /// Returns details of ApsimFile (parent record) based on the PredictedObservedDetails ID (int) of the record
    /// </summary>
    /// <param name="predictedObservedId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns the lastest (last) Pull Request Id that has been flagged as a release version,
    /// excluding the specified Pull Request Id
    /// </summary>
    /// <param name="currentPullRequestId"></param>
    /// <returns></returns>
    public static int GetLatestMergedPullRequestId(int currentPullRequestId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.ApsimFiles
                .Where(a => a.StatsAccepted == true && a.PullRequestId != currentPullRequestId)
                .OrderByDescending(a => a.RunDate)
                .Select(a => a.PullRequestId)
                .First();
        }
    }


    /// <summary>
    /// Get limited details (Pull Request ID, Run Date and Is Merged status) for all Apsim Files
    /// in reverse date order
    /// </summary>
    /// <returns></returns>
    //public static List<vApsimFile> GetAllApsimFiles()
    //{
    //    using (ApsimDBContext context = new ApsimDBContext())
    //    {
    //        return context.ApsimFiles
    //            .Select(h => new vApsimFile
    //            {
    //                PullRequestId = h.PullRequestId,
    //                RunDate = h.RunDate,
    //                SubmitDetails = h.SubmitDetails,
    //                StatsAccepted = h.StatsAccepted
    //            })
    //            .Distinct()
    //            .OrderByDescending(h => h.RunDate)
    //            .ThenByDescending(h => h.PullRequestId)
    //            .ToList();
    //    }
    //}

    public static List<vVariable> GetDistinctApsimFileNames()
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.ApsimFiles
                .Select(v => new vVariable
                {
                    Name = v.FileName,
                    Value = v.FileName
                })
                .Distinct()
                .OrderBy(v => v.Name)
                .ToList();
        }
    }
    //public static vApsimFile GetLatestAcceptedPullRequestDetails()
    //{
    //    using (ApsimDBContext context = new ApsimDBContext())
    //    {
    //        return context.ApsimFiles
    //            .Where(a => a.StatsAccepted == true)
    //            .OrderByDescending(a => a.RunDate)
    //            .ThenByDescending(a => a.PullRequestId)
    //            .Select(a => new vApsimFile
    //            {
    //                PullRequestId = a.PullRequestId,
    //                RunDate = a.RunDate,
    //                SubmitDetails = a.SubmitDetails,
    //                StatsAccepted = a.StatsAccepted
    //            })
    //            .First();
    //    }
    //}

    /// <summary>
    /// Get limited details (Pull Request ID, Run Date and Is Merged status, as well as the number of Apsim Files that
    /// make up each Pull Request, and the Percentage of these files that have Passed Tests) for all Apsim Files
    /// in reverse date order
    /// </summary>
    /// <returns></returns>
    public static List<vApsimFile> GetPullRequestsWithStatus()
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return (from pod in context.PredictedObservedDetails
                    join af in context.ApsimFiles on  pod.ApsimFilesID equals af.ID
                    select new { pod , af } into t1
                    group t1 by new {t1.af.PullRequestId, t1.af.RunDate, t1.af.SubmitDetails, t1.af.StatsAccepted } into grp
                    select new vApsimFile
                    {
                        PullRequestId = grp.FirstOrDefault().af.PullRequestId,
                        RunDate = grp.FirstOrDefault().af.RunDate,
                        SubmitDetails = grp.FirstOrDefault().af.SubmitDetails,
                        StatsAccepted = grp.FirstOrDefault().af.StatsAccepted,
                        PercentPassed = 100 * grp.Count(m => m.pod.PassedTests == 100) / grp.Count(m => m.pod.PassedTests != null),
                        Total = grp.Count(),
                        AcceptedPullRequestId = grp.FirstOrDefault().af.AcceptedPullRequestId,
                        AcceptedRunDate = grp.FirstOrDefault().af.AcceptedRunDate
                    })
                    .OrderByDescending(h => h.RunDate)
                    .ThenByDescending(h => h.PullRequestId)
                    .ToList();
        }
    }

    /// <summary>
    /// Get a list of new predicted/observed tables which are in the
    /// accepted stats but are missing from this pull request.
    /// The returned table names will be of the form "FileName.TableName".
    /// </summary>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <param name="acceptedPullRequestId">ID of the pull request of the accepted stats.</param>
    public static List<string> GetMissingTables(int pullRequestId, int acceptedPullRequestId)
    {
        string returnStr = string.Empty;
        using (ApsimDBContext context = new ApsimDBContext())
        {
            string strsql = $@"SELECT acc.[Filename]
FROM (
	SELECT 'currentPO' as 'Source', a1.[FileName] + '.' + pod1.[TableName] as [FileName]
	FROM [dbo].[ApsimFiles] a1
	INNER JOIN [dbo].[PredictedObservedDetails] pod1 on a1.ID = pod1.ApsimFilesID
	WHERE a1.[PullRequestId] = {pullRequestId}
) as curr
LEFT OUTER JOIN (
		SELECT 'acceptedPO' as 'Source', a1.[FileName] + '.' + pod1.[TableName] as [FileName]
		FROM[dbo].[ApsimFiles] a1
		INNER JOIN [dbo].[PredictedObservedDetails] pod1 on a1.ID = pod1.ApsimFilesID
		WHERE a1.[PullRequestId] = {acceptedPullRequestId}
) AS acc
ON curr.[FileName] = acc.[FileName]
WHERE curr.[FileName] IS NULL;";

            return context.Database.SqlQuery<string>(strsql).ToList();
        }
    }

    /// <summary>
    /// Get a list of new predicted/observed tables added by a pull request.
    /// The returned table names will be of the form "FileName.TableName".
    /// </summary>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <param name="acceptedPullRequestId">ID of the pull request of the accepted stats.</param>
    public static List<string> GetNewTables(int pullRequestId, int acceptedPullRequestId)
    {
        string returnStr = string.Empty;
        using (ApsimDBContext context = new ApsimDBContext())
        {
            string strsql = $@"SELECT curr.[Filename]
FROM (
	SELECT 'currentPO' as 'Source', a1.[FileName] + '.' + pod1.[TableName] as [FileName]
	FROM [dbo].[ApsimFiles] a1
	INNER JOIN [dbo].[PredictedObservedDetails] pod1 on a1.ID = pod1.ApsimFilesID
	WHERE a1.[PullRequestId] = {pullRequestId}
) as curr
LEFT OUTER JOIN (
		SELECT 'acceptedPO' as 'Source', a1.[FileName] + '.' + pod1.[TableName] as [FileName]
		FROM[dbo].[ApsimFiles] a1
		INNER JOIN [dbo].[PredictedObservedDetails] pod1 on a1.ID = pod1.ApsimFilesID
		WHERE a1.[PullRequestId] = {acceptedPullRequestId}
) AS acc
ON curr.[FileName] = acc.[FileName]
WHERE acc.[FileName] IS NULL;";

            return context.Database.SqlQuery<string>(strsql).ToList();
        }
    }

    /// <summary>
    /// Gets Details of the individual Apsim simulation Files that make up this specified Pull Request
    /// </summary>
    /// <param name="pullRequestId"></param>
    /// <returns></returns>
    public static List<vSimFile> GetSimFilesByPullRequestID(int pullRequestId)
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
                    .Where(sf => sf.ApsimFiles.PullRequestId == pullRequestId)
                    .Select(sf => new vSimFile
                    {
                        PullRequestId = sf.ApsimFiles.PullRequestId,
                        FileName = sf.ApsimFiles.FileName,
                        FullFileName = ((sf.ApsimFiles.FullFileName.Contains("GitHub")) ? sf.ApsimFiles.FullFileName.Substring(posn) : sf.ApsimFiles.FullFileName),
                        SubmitDetails = sf.ApsimFiles.SubmitDetails,
                        PredictedObservedID = sf.PredictedObservedDetails.ID,
                        strPredictedObservedID = sf.PredictedObservedDetails.ID.ToString(),
                        PredictedObservedTableName = sf.PredictedObservedDetails.TableName,
                        PassedTests = sf.PredictedObservedDetails.PassedTests,
                        AcceptedPredictedObservedDetailsID = sf.PredictedObservedDetails.AcceptedPredictedObservedDetailsID
                    })
                    .OrderBy(sf => sf.FileName)
                    .ToList();
        }
    }

    /// <summary>
    /// Gets Details of the individual Apsim simulation Files that make up this specified Pull Request and Run Date
    /// NOTE:  No longer in use as can no longer have multiple pull requests with different dates.
    /// </summary>
    /// <param name="pullRequestId"></param>
    /// <param name="runDate"></param>
    /// <returns></returns>
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
                        SubmitDetails = sf.ApsimFiles.SubmitDetails,
                        PredictedObservedID = sf.PredictedObservedDetails.ID,
                        strPredictedObservedID = sf.PredictedObservedDetails.ID.ToString(),
                        PredictedObservedTableName = sf.PredictedObservedDetails.TableName,
                        PassedTests = sf.PredictedObservedDetails.PassedTests,
                        AcceptedPredictedObservedDetailsID = sf.PredictedObservedDetails.AcceptedPredictedObservedDetailsID
                    })
                    .OrderBy(sf => sf.FileName)
                    .ToList();
        }
    }

}
