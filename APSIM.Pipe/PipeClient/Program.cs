using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
namespace PipeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT * FROM ApsimFiles";
            //cmd.Parameters.AddWithValue("@PullRequestId", 1780);

            string response = Comms.SendQuery(cmd,"reader");

            DataTable dt = JsonConvert.DeserializeObject<DataTable>(response);
        }
    }

}
