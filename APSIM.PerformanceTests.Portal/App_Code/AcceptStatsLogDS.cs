using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using APSIM.PerformanceTests.Portal.Models;


public class AcceptStatsLogDS
{
    public static AcceptStatsLog GetLatestAcceptedStatsLog()
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            //Need to ignore any records with LogStats = false, as these may have been deleted Pull Requests, or 'Updates (below).
            //Need to ignore any records with a StatsPullRequestId, as these mean that the stats were updated to this pull request,
            //and they are not a 'Stats' Accepted it.
            var acceptStats = context.AcceptStatsLogs
                .Where(a => a.LogStatus == true && a.StatsPullRequestId == 0)
                .OrderByDescending(a => a.LogAcceptDate)
                .ThenByDescending(a => a.PullRequestId)
                .FirstOrDefault();

            return acceptStats;
        }
    }
}
