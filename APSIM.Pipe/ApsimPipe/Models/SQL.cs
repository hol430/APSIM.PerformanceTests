using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;


namespace ApsimPipe.Models
{
    public static class SQL
    {

        /// <summary>
        /// Reconstruct and execute command.
        /// </summary>
        /// <param name="cmd">The received command.</param>
        /// <returns>JSON serialised response.</returns>
        public static string RelayCommand(Command cmd)
        {
            string json = "";
            Utilities.WriteToLogFile("Writing to Log file.");
            try
            {

                using (SqlConnection con = new SqlConnection(Utilities.GetConnectionString()))
                {
                    using (SqlCommand command = new SqlCommand(cmd.command, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 0;
                        foreach (KeyValuePair<string, string> p in cmd.parameters)
                        {
                            if ((cmd.type == "storedTableType") && (p.Key == cmd.paramName))
                            {
                                //Utilities.WriteToLogFile(string.Format("Ouptput Stored Proceedure {0} - Key {1}: JsonData {2}", cmd.spName, p.Key, p.Value));
                                DataTable data = (DataTable)JsonConvert.DeserializeObject(p.Value, (typeof(DataTable)));
                                SqlParameter tvpParam = command.Parameters.AddWithValue(p.Key, data);
                                tvpParam.SqlDbType = SqlDbType.Structured;
                                tvpParam.TypeName = cmd.spName;
                            }
                            else
                            {
                                command.Parameters.AddWithValue(p.Key, p.Value);
                            }
                        }

                        bool sqlSuccess = true;
                        bool mailSent = false;
                        MailMessage mail = new MailMessage("apsim@sendgrid.com", "Cla473@csiro.au");
                        SmtpClient client = new SmtpClient();
                        NetworkCredential cred = new NetworkCredential("apikey", File.ReadAllText(@"E:\Sites\APSIM-Sites\sendgrid.key"));
                        client.Port = 587;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;
                        client.Host = "smtp.sendgrid.net";
                        client.Credentials = cred;

                        // loop in case the SQL server is down
                        // Email doesn't work; need to check ports with IT
                        do
                        {
                            try
                            {
                                con.Open();
                                if (!sqlSuccess && mailSent)
                                {
                                    mail.Subject = "SQL server is back online.";
                                    try
                                    {
                                        client.Send(mail);
                                        mailSent = false;
                                    }
                                    catch (SmtpException)
                                    { }

                                    sqlSuccess = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                sqlSuccess = false;
                                mail.Subject = "SQL server is offline";
                                mail.Body = ex.Message;
                                try
                                {
                                    if (!mailSent)
                                    {
                                        client.Send(mail);
                                        mailSent = true;
                                    }
                                }
                                catch (SmtpException)
                                { }
                            }
                        } while (!sqlSuccess);

                        //JavaScriptSerializer js = new JavaScriptSerializer();
                        switch (cmd.type)
                        {
                            case "reader":
                                SqlDataReader reader = command.ExecuteReader();
                                DataTable table = new DataTable();
                                table.Load(reader);
                                //List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                                //Dictionary<string, object> row;
                                //foreach (DataRow dr in table.Rows)
                                //{
                                //    row = new Dictionary<string, object>();
                                //    foreach (DataColumn col in table.Columns)
                                //        row.Add(col.ColumnName, dr[col]);
                                //    rows.Add(row);
                                //}
                                //json = js.Serialize(rows);
                                json = JsonConvert.SerializeObject(table);
                                Utilities.WriteToLogFile(string.Format("json result: {0}", json));
                                break;
                            case "scalar":
                                object result = command.ExecuteScalar();
                                //json = js.Serialize(result);
                                json = JsonConvert.SerializeObject(result);
                                Utilities.WriteToLogFile(string.Format("json result: {0}", json));
                                break;
                            case "nonquery":
                                command.ExecuteNonQuery();
                                break;
                            case "stored":
                                command.CommandType = CommandType.StoredProcedure;
                                command.ExecuteNonQuery();
                                break;
                            case "storedTableType":
                                command.CommandType = CommandType.StoredProcedure;
                                command.ExecuteNonQuery();
                                break;
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Unable to Relay Command for {0}: {1} ", cmd.type, ex.Message.ToString()));
                throw;
            }

            return json;
        }

 
    }
}
