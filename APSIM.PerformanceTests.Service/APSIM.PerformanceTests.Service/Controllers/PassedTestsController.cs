using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace APSIM.PerformanceTests.Service.Controllers
{
    public class PassedTestsController : ApiController
    {
        //  GET (Read): api/passedtests/333
        [HttpGet]
        public bool GetPassedTestsStatus(int id)
        {
            bool hasPassed = false;

            try
            {
                string connectStr = Utilities.GetConnectionString();
                Utilities.WriteToLogFile("-----------------------------------");

                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    double PercentPassed = 0;

                    //strSQL = "SELECT a.[PullRequestId], a.[RunDate], a.[IsReleased], COUNT(CASE WHEN [PassedTests] = 100 THEN 1 ELSE NULL END), COUNT([PassedTests]), "
                    //       + " 100 * COUNT(CASE WHEN[PassedTests] = 100 THEN 1 ELSE NULL END) / COUNT([PassedTests]) as PercentPassed "
                    //       + " FROM  [dbo].[ApsimFiles] AS a "
                    //       + "    INNER JOIN[dbo].[PredictedObservedDetails] AS p ON a.ID = p.ApsimFilesID "
                    //       + "  WHERE a.[PullRequestId] = @PullRequestId "
                    //       + "  GROUP BY a.[PullRequestId], a.[RunDate], a.[IsReleased] ";

                    string strSQL = "SELECT  100 * COUNT(CASE WHEN[PassedTests] = 100 THEN 1 ELSE NULL END) / COUNT([PassedTests]) as PercentPassed "
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
                        hasPassed = true;
                    }
                }
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Unable to determine Passed/Failed status: {1}", id.ToString(), ex.Message.ToString())); ;
            }
            return hasPassed;
        }
    }
}
