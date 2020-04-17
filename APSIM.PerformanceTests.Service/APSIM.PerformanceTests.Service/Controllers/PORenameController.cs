using APSIM.PerformanceTests.Models;
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
    public class PORenameController : ApiController
    {

        [ResponseType(typeof(PORename))]
        public async Task<IHttpActionResult> PostPORename(PORename renameObj)
        {
            string errHelper = string.Empty;
            try
            {
                errHelper = string.Format("Rename of Table {0} to {1} for Apsim File {2}", renameObj.TableName, renameObj.NewTableName, renameObj.FileName);
                string authenCode = Utilities.GetStatsAcceptedToken();
                if (renameObj.SubmitUser == authenCode)
                {
                    string connectStr = Utilities.GetConnectionString();
                    using (SqlConnection sqlCon = new SqlConnection(connectStr))
                    {
                        sqlCon.Open();
                        if (renameObj.Type == "TableRename")
                        {
                            DBFunctions.RenamePOTable(sqlCon, renameObj.FileName, renameObj.TableName, renameObj.NewTableName);
                            Utilities.WriteToLogFile("    " + errHelper + "  competed successfully!");
                        }
                        else if (renameObj.Type == "VariableRename")
                        {
                            //using (SqlCommand commandENQ = new SqlCommand("usp_UpdatePredictedObservedVariableName", sqlCon))
                            //{
                            //    //Now update the database with the test results
                            //    // Configure the command and parameter.
                            //    commandENQ.CommandType = CommandType.StoredProcedure;
                            //    commandENQ.Parameters.AddWithValue("@FileName", renameObj.FileName);
                            //    commandENQ.Parameters.AddWithValue("@TableName", renameObj.TableName);
                            //    commandENQ.Parameters.AddWithValue("@VariableName", renameObj.VariableName);
                            //    commandENQ.Parameters.AddWithValue("@NewVariableName", renameObj.NewVariableName);

                            //    commandENQ.ExecuteNonQuery();
                            //}
                            //Utilities.WriteToLogFile("    " + errHelper + "  competed successfully!");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  {0} Failed: {1}", errHelper, ex.Message.ToString())); ;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}