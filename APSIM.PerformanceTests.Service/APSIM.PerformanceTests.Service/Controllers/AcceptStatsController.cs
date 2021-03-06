﻿using APSIM.PerformanceTests.Models;
using APSIM.Shared.Utilities;
using Octokit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.IO;
using System.Web.Http;
using System.Threading.Tasks;
using System.Text;
using System.Web.Http.Description;


namespace APSIM.PerformanceTests.Service.Controllers
{
    public class AcceptStatsController : ApiController
    {
        /// <summary>
        /// Determines whether or not (returning true/false), the PredictdObservedValues for all Apsim Simulation files Pass the testing criteria
        /// for a specific Pull Request Id
        ///  Usage:  POST (Read): api/acceptstats/333
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetAcceptedStatsStatusAndUpdateGithub(int id)
        {
            Utilities.WriteToLogFile("-----------------------------------");
            double PercentPassed = 0;
            bool passed = false;
            int acceptedFileCount = 0;
            int currentFileCount = 0;
            string connectStr = Utilities.GetConnectionString();
            using (SqlConnection sqlCon = new SqlConnection(connectStr))
            {
                sqlCon.Open();
                try
                {
                    // Get proportion of tests which passed.
                    string strSQL = "SELECT  100 * COUNT(CASE WHEN [PassedTests] = 100 THEN 1 ELSE NULL END) / COUNT(CASE WHEN [PassedTests] IS NOT NULL  THEN 1 ELSE 0 END) as PercentPassed "
                                  + " FROM  [dbo].[ApsimFiles] AS a "
                                  + "    INNER JOIN[dbo].[PredictedObservedDetails] AS p ON a.ID = p.ApsimFilesID "
                                  + "  WHERE a.[PullRequestId] = @PullRequestId ";
                    using (SqlCommand commandES = new SqlCommand(strSQL, sqlCon))
                    {
                        commandES.CommandType = CommandType.Text;
                        commandES.Parameters.AddWithValue("@PullRequestId", id);
                        object obj = commandES.ExecuteScalar();
                        PercentPassed = double.Parse(obj.ToString());
                    }

                    // Get file count for accepted stats.
                    strSQL = "SELECT TOP (1) [FileCount]" + Environment.NewLine +
                                "FROM[APSIM.PerformanceTests].[dbo].[AcceptStatsLogs] af" + Environment.NewLine +
                                "WHERE af.[LogStatus] = 1" + Environment.NewLine +
                                "AND af.[StatsPullRequestId] = 0" + Environment.NewLine +
                                "ORDER BY ID DESC";
                    using (SqlCommand commandES = new SqlCommand(strSQL, sqlCon))
                    {
                        commandES.CommandType = CommandType.Text;
                        object obj = commandES.ExecuteScalar();
                        acceptedFileCount = int.Parse(obj.ToString());
                    }

                    // Get file count for this pull request.
                    strSQL = @"SELECT COUNT(*) as CurrentFileCount
FROM [APSIM.PerformanceTests].[dbo].[PredictedObservedDetails], [APSIM.PerformanceTests].[dbo].[ApsimFiles] af
WHERE [ApsimFilesID] = af.[ID]
AND af.[PullRequestId] = @PullRequestID";
                    using (SqlCommand commandES = new SqlCommand(strSQL, sqlCon))
                    {
                        commandES.CommandType = CommandType.Text;
                        commandES.Parameters.AddWithValue("@PullRequestId", id);
                        object obj = commandES.ExecuteScalar();
                        currentFileCount = int.Parse(obj.ToString());
                    }

                    if (PercentPassed == 100 && currentFileCount == acceptedFileCount)
                    {
                        passed = true;
                    }
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Unable to determine Passed/Failed status: {1}", id.ToString(), ex.Message.ToString())); ;
                }
                CallGitHubWithPassFail(id, passed);
                Utilities.WriteToLogFile(string.Format("   Pull Request Id {0}, PassedTestsStatus verified and Github updated.", id.ToString())); ;
            }
            return Ok();
        }

        [ResponseType(typeof(AcceptStatsLog))]
        public async Task<IHttpActionResult> PostAcceptStats(AcceptStatsLog acceptLog)
        {
            try
            {
                string authenCode = Utilities.GetStatsAcceptedToken();
                if (acceptLog.LogPerson == authenCode)
                {
                    //update the 'Stats accepted column from here
                    DBFunctions.UpdateAsStatsAccepted("Accept", acceptLog);
                    CallGitHubWithPassFail(acceptLog.PullRequestId, acceptLog.LogStatus);
                    Utilities.WriteToLogFile(string.Format("   Pull Request Id {0}, AcceptedStats has been confirmed and Github updated.", acceptLog.PullRequestId.ToString())); ;
                }
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Unable to update AcceptedStats status: {1}", acceptLog.PullRequestId.ToString(), ex.Message.ToString())); ;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }


        private static void CallGitHubWithPassFail(int pullRequestID, bool pass)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("ApsimX"));
            string token = Utilities.GetGitHubToken();
            github.Credentials = new Credentials(token);
            Task<PullRequest> pullRequestTask = github.PullRequest.Get("APSIMInitiative", "ApsimX", pullRequestID);
            pullRequestTask.Wait();
            PullRequest pullRequest = pullRequestTask.Result;
            Uri statusURL = new System.Uri(pullRequest.StatusesUrl);

            string header = "Authorization: token " + token;
            string state = "failure";
            string stateFormatted = "Fail";
            if (pass)
            {
                state = "success";
                stateFormatted = "Pass";
            }

            string urlStr = string.Format("https://apsim.csiro.au/APSIM.PerformanceTests/Default.aspx?PULLREQUEST={0}", pullRequestID);

            string body = "{" + Environment.NewLine +
                          "  \"state\": \"" + state + "\"," + Environment.NewLine +
                          "  \"target_url\": \"" + urlStr + "\"," + Environment.NewLine +
                          "  \"description\": \"" + stateFormatted + "\"," + Environment.NewLine +
                          "  \"context\": \"APSIM.PerformanceTests\"" + Environment.NewLine +
                          "}";

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(body);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(statusURL);
            webRequest.Method = "POST";
            webRequest.ContentType = @"application/x-www-form-urlencoded";
            webRequest.ContentLength = byte1.Length;
            webRequest.Headers.Add(header);
            webRequest.UserAgent = "dummy";
            using (Stream s = webRequest.GetRequestStream())
                s.Write(byte1, 0, byte1.Length);
            webRequest.GetResponse();
        }

    }
}
