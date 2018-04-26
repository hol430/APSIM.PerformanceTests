using System.Web.Script.Serialization;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using Newtonsoft.Json;


namespace APSIM.PerformanceTests.Service
{
    public static class Comms
    {
        /// <summary>
        /// Send a completed query to the server
        /// </summary>
        /// <param name="command">The SqlCommand to send.</param>
        /// <param name="type">The type of command: reader, scalar, nonquery, stored.</param>
        /// <returns></returns>
        public static string SendQuery(SqlCommand command, string type)
        {
            Command cmd = new Command();
            cmd.command = command.CommandText;
            cmd.type = type;
            foreach (SqlParameter p in command.Parameters)
            {
                cmd.parameters.Add(p.ParameterName, p.SqlValue.ToString());
            }
            return SendData(new JavaScriptSerializer().Serialize(cmd));
        }

        public static string SendQuerySP(SqlCommand command, string type, string paramName, DataTable spData, string spName)
        {
            Command cmd = new Command();
            cmd.command = command.CommandText;
            cmd.type = type;
            foreach (SqlParameter p in command.Parameters)
            {
                if ((cmd.type == "storedTableType") && (p.ParameterName == paramName))
                {
                    cmd.paramName = paramName;
                    cmd.spName = spName;
                    string strData = JsonConvert.SerializeObject(spData);
                    //cmd.spData = JsonConvert.SerializeObject(spData);
                    cmd.parameters.Add(p.ParameterName, strData);
                }
                else
                {
                    cmd.parameters.Add(p.ParameterName, p.SqlValue.ToString());
                }
            }
            return SendData(new JavaScriptSerializer().Serialize(cmd));
        }

        /// <summary>
        /// Sends a serialised JSON string to the server.
        /// </summary>
        /// <param name="json">A serialised Command object.</param>
        /// <returns>The query response in JSON format.</returns>
        private static string SendData(string json)
        {
            string response = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://apsim.csiro.au/APSIM.Pipe/api/data");
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version11;
                request.ContentType = "application/json";
                request.Method = "POST";

                //send the data to the server
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }
            }
            catch (System.Exception ex)
            {
                Utilities.WriteToLogFile("ERROR sending data to apsim.csiro.au/APSIM.Pipe/api/data: " + ex.Message.ToString());
            }

            return response;
        }
    }
}
