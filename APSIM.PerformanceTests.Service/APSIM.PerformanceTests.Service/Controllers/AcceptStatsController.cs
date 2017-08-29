using APSIM.PerformanceTests.Models;
using Octokit;
using System;
using System.Data;
using System.Data.SqlClient;
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
            try
            {
                string connectStr = Utilities.GetConnectionString();
                Utilities.WriteToLogFile("-----------------------------------");

                bool passed = false;
                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    double PercentPassed = 0;

                    string strSQL = "SELECT  100 * COUNT(CASE WHEN [PassedTests] = 100 THEN 1 ELSE NULL END) / COUNT(CASE WHEN [PassedTests] IS NOT NULL  THEN 1 ELSE 0 END) as PercentPassed "
                                  + " FROM  [dbo].[ApsimFiles] AS a "
                                  + "    INNER JOIN[dbo].[PredictedObservedDetails] AS p ON a.ID = p.ApsimFilesID "
                                  + "  WHERE a.[PullRequestId] = @PullRequestId ";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", id);
                        con.Open();
                        object obj = command.ExecuteScalar();
                        PercentPassed = double.Parse(obj.ToString());
                        con.Close();
                    }
                    if (PercentPassed == 100)
                    {
                        passed = true;
                    }
                }
                CallGitHubWithPassFail(id, passed);
                Utilities.WriteToLogFile(string.Format("   Pull Request Id {0}, PassedTestsStatus verified and Github updated.", id.ToString())); ;
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Unable to determine Passed/Failed status: {1}", id.ToString(), ex.Message.ToString())); ;
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
                    UpdateAsStatsAccepted(acceptLog);
                    CallGitHubWithPassFail(acceptLog.PullRequestId, acceptLog.LogStatus);
                    Utilities.WriteToLogFile(string.Format("   Pull Request Id {0}, AcceptedStats has been confirmed and Github updated.", acceptLog.PullRequestId.ToString())); ;
                }
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Unable to update AcceptedStats status: {1}", acceptLog.PullRequestId.ToString(), ex.Message.ToString())); ;
            }
            return Ok();
        }



        private static void CallGitHubWithPassFail(int pullRequestID, bool pass)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("ApsimX"));
            string token = Utilities.GetGitHubToken();
            github.Credentials = new Credentials(token);
            Task<PullRequest> pullRequestTask = github.PullRequest.Get("APSIMInitiative", "ApsimX", pullRequestID);
            pullRequestTask.Wait();
            PullRequest pullRequest = pullRequestTask.Result;
            Uri statusURL = pullRequest.StatusesUrl;

            string header = "Authorization: token " + token;
            string state = "failure";
            string stateFormatted = "Fail";
            if (pass)
            {
                state = "success";
                stateFormatted = "Pass";
            }

            string urlStr = string.Format("http://www.apsim.info/APSIM.PerformanceTests.Portal/Default.aspx?PULLREQUEST={0}", pullRequestID);

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


        private void UpdateAsStatsAccepted(AcceptStatsLog acceptLog)
        {
            try
            {
                string connectStr = Utilities.GetConnectionString();
                Utilities.WriteToLogFile("-----------------------------------");

                //need to authenticate the process

                int statsAccepted = Convert.ToInt32(acceptLog.LogStatus);
                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    string strSQL = "INSERT INTO AcceptStatsLog (PullRequestId, SubmitPerson, SubmitDate, LogPerson, LogReason, LogStatus) "
                                  + " Values ( @PullRequestId, @SubmitPerson, @SubmitDate, @LogPerson, @LogReason, @LogStatus )";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", acceptLog.PullRequestId);
                        command.Parameters.AddWithValue("@SubmitPerson", acceptLog.SubmitPerson);
                        command.Parameters.AddWithValue("@SubmitDate", acceptLog.SubmitDate);
                        command.Parameters.AddWithValue("@LogPerson", acceptLog.LogPerson);
                        command.Parameters.AddWithValue("@LogReason", acceptLog.LogReason);
                        command.Parameters.AddWithValue("@LogStatus", acceptLog.LogStatus);

                        con.Open();
                        command.ExecuteNonQuery();
                        con.Close();
                    }

                    strSQL = "UPDATE ApsimFiles SET StatsAccepted = @StatsAccepted, IsMerged = @IsMerged WHERE PullRequestId = @PullRequestId";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@StatsAccepted", statsAccepted);
                        command.Parameters.AddWithValue("@IsMerged", statsAccepted);        //do this the same to during changeover
                        command.Parameters.AddWithValue("@PullRequestId", acceptLog.PullRequestId);

                        con.Open();
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                }
                //Utilities.WriteToLogFile(string.Format("    Accept Stats Status updated to {0} by {1} on {2}. Reason: {3}", acceptLog.LogStatus, acceptLog.LogPerson, acceptLog.SubmitDate, acceptLog.LogReason));
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Failed to update as 'Stats Accepted': {1}", acceptLog.PullRequestId.ToString(), ex.Message.ToString()));
            }
        }

    }
}
