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

        //  POST (Save): api/Release/333/true  (was a put)
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
