using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;


namespace APSIM.PerformanceTests.Service.Controllers
{
    public class ReleaseController : ApiController
    {

        /// <summary>
        ///  GEts the IsRelease status for a specific Pull Request Id
        ///  Usage:  GET : api/release/333
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public bool GetIsReleasedStatus(int id)
        {
            bool IsRelease = false;
            try
            {
                string connectStr = Utilities.GetConnectionString();
                Utilities.WriteToLogFile("-----------------------------------");

                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    string strSQL = "SELECT TOP 1 a.[IsReleased] "
                                  + " FROM  [dbo].[ApsimFiles] AS a "
                                  + "    INNER JOIN[dbo].[PredictedObservedDetails] AS p ON a.ID = p.ApsimFilesID "
                                  + "  WHERE a.[PullRequestId] = @PullRequestId ";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", id);
                        con.Open();
                        object obj = command.ExecuteScalar();
                        IsRelease = bool.Parse(obj.ToString());
                        con.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Unable to get the IsRelease status: {1}", id.ToString(), ex.Message.ToString()));
            }
            return IsRelease;
        }


        /// <summary>
        ///  Updates the IsRelease status for a specific Pull Request
        ///  Usage:  POST (Save): api/Release/333/true  (was a put)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="releaseStatus"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> PostIsReleased(int id, bool releaseStatus)
        {
            try
            {
                string connectStr = Utilities.GetConnectionString();
                Utilities.WriteToLogFile("-----------------------------------");

                int IsReleased = Convert.ToInt32(releaseStatus);
                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    string strSQL = "UPDATE ApsimFiles SET IsReleased = @IsReleased WHERE PullRequestId = @PullRequestId";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@IsReleased", IsReleased);
                        command.Parameters.AddWithValue("@PullRequestId", id);
                        con.Open();
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                }
                Utilities.WriteToLogFile(string.Format("Pull Request Id {0}, updated IsReleased as {1} on {2}!", id.ToString(), releaseStatus.ToString(), System.DateTime.Now.ToString("dd/mm/yyyy HH:mm")));
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Failed to update as Release version: {1}", id.ToString(), ex.Message.ToString()));
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
