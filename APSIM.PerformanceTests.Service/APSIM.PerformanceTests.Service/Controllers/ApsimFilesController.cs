using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Web.Http;
using System.Web.Http.Description;
using APSIM.PerformanceTests.Models;
using System.Threading.Tasks;
using System.Text;
using APSIM.Shared.Utilities;


namespace APSIM.PerformanceTests.Service.Controllers
{

    public class ApsimFilesController : ApiController
    {

        //  GET (Read): api/apsimfiles/ 
        public List<ApsimFile> GetAllApsimFiles()
        {
            List<ApsimFile> apsimFiles = new List<ApsimFile>();
            string connectStr = Utilities.GetConnectionString();

            using (SqlConnection con = new SqlConnection(connectStr))
            {
                string strSQL = "SELECT * FROM ApsimFiles";
                using (SqlCommand command = new SqlCommand(strSQL, con))
                {
                    command.CommandType = CommandType.Text;
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ApsimFile apsim = new ApsimFile();
                        apsim.ID = reader.GetInt32(0);
                        apsim.PullRequestId = reader.GetInt32(1);
                        apsim.FileName = reader.GetString(2);
                        apsim.FullFileName = reader.GetString(3);
                        apsim.RunDate = reader.GetDateTime(4);
                        apsim.IsReleased = reader.GetBoolean(5);
                        apsimFiles.Add(apsim);
                    }
                    con.Close();
                }
            }
            return apsimFiles;
        }


        //  GET (Read): api/apsimfiles/5 
        [ResponseType(typeof(ApsimFile))]
        public List<ApsimFile> GetApsimFile(int id)
        {
            List<ApsimFile> apsimFiles = new List<ApsimFile>();
            string connectStr = Utilities.GetConnectionString();

            using (SqlConnection con = new SqlConnection(connectStr))
            {
                string strSQL = "SELECT * FROM ApsimFiles WHERE PullRequestId = @PullRequestId";
                using (SqlCommand command = new SqlCommand(strSQL, con))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@PullRequestId", id);
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ApsimFile apsim = new ApsimFile();
                        apsim.ID = reader.GetInt32(0);
                        apsim.PullRequestId = reader.GetInt32(1);
                        apsim.FileName = reader.GetString(2);
                        apsim.FullFileName = reader.GetString(3);
                        apsim.RunDate = reader.GetDateTime(4);
                        apsim.IsReleased = reader.GetBoolean(5);
                        apsimFiles.Add(apsim);
                    }
                    con.Close();
                }
            }
            return apsimFiles;
        }

        //  GET (Read): api/apsimfiles/333/true  (was a put)
        public List<ApsimFile> GetApsimFileUpdatedIsReleased(int id, bool releaseStatus)
        {
            List<ApsimFile> apsimFiles = new List<ApsimFile>();

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
                apsimFiles = GetApsimFile(id);
                Utilities.WriteToLogFile(string.Format("Pull Request Id {0}, updated IsReleased as {1} on {2}!", id.ToString(), releaseStatus.ToString(), System.DateTime.Now.ToString("dd/mm/yyyy HH:mm")));
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Failed to update as Release version: {1}", id.ToString(), ex.Message.ToString()));
            }
            return apsimFiles; ;

        }

        // PUT (Update): api/apsimfiles/1638/true
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutApsimFileIsReleased(int id, bool releaseStatus)
        {
            try
            {
                //do the changes here
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
                return StatusCode(HttpStatusCode.NoContent);
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, Failed to update as Release version: {1}", id.ToString(), ex.Message.ToString()));
                return StatusCode(HttpStatusCode.NoContent);
            }
        }

        //DELETE : api/apsimfiles/ID
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteByPullRequestId(int id)
        {
            try
            {
                string connectStr = Utilities.GetConnectionString();

                Utilities.WriteToLogFile("-----------------------------------");
                DeleteByPullRequest(connectStr, id);
                Utilities.WriteToLogFile(string.Format("Pull Request Id {0}, deleted on {1}!", id.ToString(), System.DateTime.Now.ToString("dd/mm/yyyy HH:mm")));

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Unable to delete Pull Request Id {0}: {1}", id.ToString(), ex.Message.ToString()));
                return StatusCode(HttpStatusCode.NoContent);
            }
        }



        // POST (Create): api/apsimfiles/ apsimfile
        [ResponseType(typeof(ApsimFile))]
        public async Task<IHttpActionResult> PostApsimFile(ApsimFile apsimfile)
        {
            int ApsimID = 0;
            string ErrMessageHelper = string.Empty;

            try
            {
                string connectStr = Utilities.GetConnectionString();
                string strSQL;

                Utilities.WriteToLogFile("  ");
                Utilities.WriteToLogFile("==========================================================");
                Utilities.WriteToLogFile(string.Format("Processing PullRequestId {0}, Apsim Filename {1}, dated {2}!", apsimfile.PullRequestId, apsimfile.FileName, apsimfile.RunDate.ToString("dd/mm/yyyy HH:mm")));


                //--------------------------------------------------------------------------------------
                //Need to check if this Pull Request Id has already been used,  if it has, then we need
                //to delete everything associated with it before we save the new set of data
                //--------------------------------------------------------------------------------------
                int pullRequestCount = 0;
                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    strSQL = "SELECT COUNT(ID) FROM ApsimFiles WHERE PullRequestId = @PullRequestId";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", apsimfile.PullRequestId);

                        con.Open();
                        pullRequestCount = (int)command.ExecuteScalar();
                        con.Close();
                    }
                }
                if (pullRequestCount > 0)
                {
                    DeleteByPullRequest(connectStr, apsimfile.PullRequestId);
                    Utilities.WriteToLogFile("    Removed original Pull Request Data.");
                }

                //--------------------------------------------------------------------------------------
                //Add the ApsimFile Record first, so that we can get back the IDENTITY (ID) value
                //--------------------------------------------------------------------------------------
                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    strSQL = "INSERT INTO ApsimFiles (PullRequestId, FileName, FullFileName, RunDate, IsReleased) OUTPUT INSERTED.ID Values (@PullRequestId, @FileName, @FullFileName, @RunDate, @IsReleased)";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", apsimfile.PullRequestId);
                        command.Parameters.AddWithValue("@FileName", apsimfile.FileName);
                        command.Parameters.AddWithValue("@FullFileName", Utilities.GetModifiedFileName(apsimfile.FullFileName));
                        command.Parameters.AddWithValue("@RunDate", apsimfile.RunDate);
                        command.Parameters.AddWithValue("@IsReleased", apsimfile.IsReleased);


                        //this should return the IDENTITY value for this record (which is required for the next update)
                        ErrMessageHelper = "Filename: " + apsimfile.FileName;
                        con.Open();
                        ApsimID = (int)command.ExecuteScalar();
                        con.Close();
                        ErrMessageHelper = "Filename: " + apsimfile.FileName + "- ApsimID: " + ApsimID;
                        Utilities.WriteToLogFile(string.Format("    Filename {0} imported successfully!", apsimfile.FileName));
                    }
                }
                //--------------------------------------------------------------------------------------
                //Add the Simulation Details to the database 
                //--------------------------------------------------------------------------------------

                if (apsimfile.Simulations.Rows.Count > 0)
                {

                    //--------------------------------------------------------------------------------------
                    using (SqlConnection con = new SqlConnection(connectStr))
                    {
                        // Configure the command and parameter.
                        using (SqlCommand command = new SqlCommand("usp_SimulationsInsert", con))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@ApsimID", ApsimID);

                            SqlParameter tvpParam = command.Parameters.AddWithValue("@Simulations", apsimfile.Simulations);
                            tvpParam.SqlDbType = SqlDbType.Structured;
                            tvpParam.TypeName = "dbo.SimulationDataTableType";

                            ErrMessageHelper = "- Simualtion Data for " + apsimfile.FileName;
                            con.Open();
                            command.ExecuteNonQuery();
                            con.Close();
                            Utilities.WriteToLogFile(string.Format("    Filename {0} Simulation Data imported successfully!", apsimfile.FileName));
                        }
                    }
                }


                //--------------------------------------------------------------------------------------
                //Add the Predited Observed Details (MetaData) and then the data
                //--------------------------------------------------------------------------------------

                //now look at each individual set of data
                foreach (PredictedObservedDetails poDetail in apsimfile.PredictedObserved)
                {
                    int predictedObservedID;
                    using (SqlConnection con = new SqlConnection(connectStr))
                    {
                        strSQL = "INSERT INTO PredictedObservedDetails ("
                        + " ApsimFilesID, TableName, PredictedTableName, ObservedTableName, FieldNameUsedForMatch, FieldName2UsedForMatch, FieldName3UsedForMatch, HasTests "
                        + " ) OUTPUT INSERTED.ID Values ("
                        + " @ApsimFilesID, @TableName, @PredictedTableName, @ObservedTableName, @FieldNameUsedForMatch, @FieldName2UsedForMatch, @FieldName3UsedForMatch, 0 "
                        + " )";

                        using (SqlCommand command = new SqlCommand(strSQL, con))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@ApsimFilesID", ApsimID);
                            command.Parameters.AddWithValue("@TableName", poDetail.DatabaseTableName);
                            command.Parameters.AddWithValue("@PredictedTableName", poDetail.PredictedTableName);
                            command.Parameters.AddWithValue("@ObservedTableName", poDetail.ObservedTableName);

                            command.Parameters.AddWithValue("@FieldNameUsedForMatch", poDetail.FieldNameUsedForMatch);
                            command.Parameters.AddWithValue("@FieldName2UsedForMatch", poDetail.FieldName2UsedForMatch);
                            command.Parameters.AddWithValue("@FieldName3UsedForMatch", poDetail.FieldName3UsedForMatch);

                            //this should return the IDENTITY value for this record (which is required for the next update)
                            //NEED TO UNDO THIS
                            ErrMessageHelper = "PredictedObservedDetails for " + poDetail.DatabaseTableName;
                            con.Open();
                            predictedObservedID = (int)command.ExecuteScalar();
                            con.Close();
                            ErrMessageHelper = "PredictedObservedDetails for " + poDetail.DatabaseTableName + "(ID: " + predictedObservedID + ")";
                            Utilities.WriteToLogFile(string.Format("    Filename {0} PredictedObserved Table Details {1}, (Id: {2}) imported successfully!", apsimfile.FileName, poDetail.DatabaseTableName, predictedObservedID));
                        }
                    }

                    //--------------------------------------------------------------------------------------
                    //And finally this is where we will insert the actual Predited Observed DATA
                    //--------------------------------------------------------------------------------------
                    DataView PredictedObservedView = new DataView(poDetail.PredictedObservedData);

                    string ObservedColumName, PredictedColumnName;

                    Utilities.WriteToLogFile(string.Format("    PredictedObserved Data Values for  {0}.{1}, (Id: {2}) import started!", apsimfile.FileName, poDetail.DatabaseTableName, predictedObservedID));

                    //need to find the first (and then each instance thereafter) of a field name being with Observed,
                    //the get the corresponding Predicted field name, and then create a new table definition based on this
                    //data,
                    for (int i = 0; i < poDetail.PredictedObservedData.Columns.Count; i++)
                    {
                        ObservedColumName = poDetail.PredictedObservedData.Columns[i].ColumnName.Trim();
                        if (ObservedColumName.StartsWith("Observed"))
                        {
                            //get the corresponding Predicted Column Name
                            int dotPosn = ObservedColumName.IndexOf('.');
                            string valueName = ObservedColumName.Substring(dotPosn + 1);
                            PredictedColumnName = "Predicted." + valueName;

                            DataTable selectedData;

                            if (poDetail.FieldName3UsedForMatch.Length > 0)
                            {
                                selectedData = PredictedObservedView.ToTable(false, "SimulationID", poDetail.FieldNameUsedForMatch, poDetail.FieldName2UsedForMatch, poDetail.FieldName3UsedForMatch, PredictedColumnName, ObservedColumName);
                                strSQL = "usp_PredictedObservedDataThreeInsert";
                            }
                            else if (poDetail.FieldName2UsedForMatch.Length > 0)
                            {
                                selectedData = PredictedObservedView.ToTable(false, "SimulationID", poDetail.FieldNameUsedForMatch, poDetail.FieldName2UsedForMatch, PredictedColumnName, ObservedColumName);
                                strSQL = "usp_PredictedObservedDataTwoInsert";
                            }
                            else
                            {
                                selectedData = PredictedObservedView.ToTable(false, "SimulationID", poDetail.FieldNameUsedForMatch, PredictedColumnName, ObservedColumName);
                                strSQL = "usp_PredictedObservedDataInsert";
                            }


                            using (SqlConnection con = new SqlConnection(connectStr))
                            {
                                using (SqlCommand command = new SqlCommand(strSQL, con))
                                {
                                    command.CommandType = CommandType.StoredProcedure;
                                    command.Parameters.AddWithValue("@PredictedObservedID", predictedObservedID);
                                    command.Parameters.AddWithValue("@ApsimFilesID", ApsimID);
                                    command.Parameters.AddWithValue("@ValueName", valueName);

                                    if (poDetail.FieldName3UsedForMatch.Length > 0)
                                    {
                                        command.Parameters.AddWithValue("@MatchName", poDetail.FieldNameUsedForMatch);
                                        command.Parameters.AddWithValue("@MatchName2", poDetail.FieldName2UsedForMatch);
                                        command.Parameters.AddWithValue("@MatchName3", poDetail.FieldName3UsedForMatch);
                                    }
                                    else if (poDetail.FieldName2UsedForMatch.Length > 0)
                                    {
                                        command.Parameters.AddWithValue("@MatchName", poDetail.FieldNameUsedForMatch);
                                        command.Parameters.AddWithValue("@MatchName2", poDetail.FieldName2UsedForMatch);
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@MatchName", poDetail.FieldNameUsedForMatch);
                                    }

                                    SqlParameter tvtpPara = command.Parameters.AddWithValue("@PredictedOabservedData", selectedData);
                                    tvtpPara.SqlDbType = SqlDbType.Structured;

                                    if (poDetail.FieldName3UsedForMatch.Length > 0)
                                    {
                                        tvtpPara.TypeName = "dbo.PredictedObservedDataThreeTableType";
                                    }
                                    else if (poDetail.FieldName2UsedForMatch.Length > 0)
                                    {
                                        tvtpPara.TypeName = "dbo.PredictedObservedDataTwoTableType";
                                    }
                                    else
                                    {
                                        tvtpPara.TypeName = "dbo.PredictedObservedDataTableType";
                                    }

                                    try
                                    {
                                        ErrMessageHelper = "PredictedObservedDetails Id " + predictedObservedID + ", ValueName: " + valueName;
                                        con.Open();
                                        command.ExecuteNonQuery();
                                        con.Close();
                                        Utilities.WriteToLogFile(string.Format("       PredictedObserved Data for {0} import completed successfully!", valueName));
                                    }
                                    catch (Exception ex)
                                    {
                                        Utilities.WriteToLogFile("    ERROR:  Unable to import PredictedObserved Data: " + ErrMessageHelper.ToString() + " - " + ex.Message.ToString());
                                    }
                                }
                            }
                        }   //ObservedColumName.StartsWith("Observed")
                    }   // for (int i = 0; i < poDetail.PredictedObservedData.Columns.Count; i++)


                    //Need to run the testing procecedure here, and then save the test data
                    if (poDetail.PredictedObservedData.Rows.Count > 0)
                    {
                        ErrMessageHelper = string.Empty;
                        Utilities.WriteToLogFile(string.Format("    Tests Data for {0}.{1} import started.", apsimfile.FileName, poDetail.DatabaseTableName));

                        //need to retrieve data for the "IsReleased" version, so that we can update the stats
                        //Get the 'IsRelease' Pull request id
                        int acceptedPredictedObservedDetailsID = 0;    //this should get updated in 'RetrieveAcceptedStatsData' 
                        DataTable acceptedStats = RetrieveAcceptedStatsData(connectStr, apsimfile, poDetail, predictedObservedID, ref acceptedPredictedObservedDetailsID);

                        DataTable dtTests = Tests.DoValidationTest(poDetail.DatabaseTableName, poDetail.PredictedObservedData, acceptedStats);

                        if (dtTests.Rows.Count > 0)
                        {
                            using (SqlConnection con = new SqlConnection(connectStr))
                            {
                                using (SqlCommand command = new SqlCommand("usp_PredictedObservedTestsInsert", con))
                                {
                                    //Now update the database with the test results
                                    // Configure the command and parameter.
                                    command.CommandType = CommandType.StoredProcedure;
                                    command.Parameters.AddWithValue("@PredictedObservedID", predictedObservedID);

                                    SqlParameter tvpParam = command.Parameters.AddWithValue("@Tests", dtTests);
                                    tvpParam.SqlDbType = SqlDbType.Structured;
                                    tvpParam.TypeName = "dbo.PredictedObservedTestsTableType";

                                    try
                                    {
                                        con.Open();
                                        command.ExecuteNonQuery();
                                        con.Close();
                                        Utilities.WriteToLogFile(string.Format("    Tests Data for {0}.{1} import completed successfully!", apsimfile.FileName, poDetail.DatabaseTableName));
                                    }
                                    catch (Exception ex)
                                    {
                                        Utilities.WriteToLogFile(string.Format("    ERROR: Unable to save Tests Data for {0}.{1}:  {2}", apsimfile.FileName, poDetail.DatabaseTableName, ex.Message.ToString()));
                                    }
                                }
                            }
                        }
                        else
                        {
                            Utilities.WriteToLogFile(string.Format("    ERROR:  Tests Data for {0}.{1} does not exist.", apsimfile.FileName, poDetail.DatabaseTableName));
                        }

                        //Update the accepted reference for Predicted Observed Values, so that it can be 
                        if (acceptedPredictedObservedDetailsID > 0 && predictedObservedID > 0)
                        {
                            using (SqlConnection con = new SqlConnection(connectStr))
                            {
                                strSQL = "UPDATE PredictedObservedDetails "
                                       + " SET AcceptedPredictedObservedDetailsID = @AcceptedPredictedObservedDetailsID "
                                       + " WHERE ID = @PredictedObservedDetailsID ";

                                using (SqlCommand command = new SqlCommand(strSQL, con))
                                {
                                    command.CommandType = CommandType.Text;
                                    command.Parameters.AddWithValue("@AcceptedPredictedObservedDetailsID", acceptedPredictedObservedDetailsID);
                                    command.Parameters.AddWithValue("@PredictedObservedDetailsID", predictedObservedID);

                                    ErrMessageHelper = "- Updating 'Accepted' PredictedObservedDetailsID for Current PredictedObservedDetails.";
                                    con.Open();
                                    command.ExecuteNonQuery();
                                    con.Close();
                                }
                            }

                            //NOT sure the following is 'necessary' - NOT DONE YET
                            //TODO: Need to update the PredictedObservedValues 'Accepted' reference agains the 'Current' PredictedObservedValues.
                            //using (SqlConnection con = new SqlConnection(connectStr))
                            //{
                            //    strSQL = "usp_UpdatePredictedObservedValues_AcceptedID";
                            //    using (SqlCommand command = new SqlCommand(strSQL, con))
                            //    {
                            //        command.CommandType = CommandType.StoredProcedure;
                            //        command.Parameters.AddWithValue("@AcceptedPredictedObservedDetailsID", acceptedPredictedObservedDetailsID);
                            //        command.Parameters.AddWithValue("@CurrentPredictedObservedDetailsID", predictedObservedID);

                            //        ErrMessageHelper = "- Updating 'Accepted' PredictedObservedValuesID for Current PredictedObservedValues.";
                            //        con.Open();
                            //        command.ExecuteNonQuery();
                            //        con.Close();
                            //    }
                            //}

                        }

                    }

                }   //foreach (PredictedObservedDetails poDetail in apsimfile.PredictedObserved)

                return CreatedAtRoute("DefaultApi", new { id = ApsimID }, apsimfile);
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile("    ERROR:  Unable to update SQL Server: " + ErrMessageHelper.ToString() + " - " + ex.Message.ToString());
                throw new Exception("Unable to update SQL Server: " + ex.Message.ToString());
            }
        }


        private static DataTable RetrieveAcceptedStatsData(string conStr, ApsimFile currentApsim, PredictedObservedDetails poDetail, int predictedObservedId, ref int acceptedPredictedObservedDetailsID)
        {
            try
            {
                DataTable acceptedStats = new DataTable();
                ApsimFile acceptedApsim = new ApsimFile();

                string strSQL = "SELECT TOP 1 * FROM ApsimFiles WHERE IsReleased = 1 AND PullRequestId != @PullRequestId ORDER BY RunDate DESC";
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", currentApsim.PullRequestId);
                        con.Open();

                        SqlDataReader sdReader = command.ExecuteReader();
                        while (sdReader.Read())
                        {
                            acceptedApsim.ID = sdReader.GetInt32(0);
                            acceptedApsim.PullRequestId = sdReader.GetInt32(1);
                            acceptedApsim.FileName = sdReader.GetString(2);
                            acceptedApsim.FullFileName = sdReader.GetString(3);
                            acceptedApsim.RunDate = sdReader.GetDateTime(4);
                            acceptedApsim.IsReleased = sdReader.GetBoolean(5);
                        }
                        con.Close();

                    }
                };

                if (acceptedApsim.PullRequestId > 0)
                {
                    ////get the PredictedObservedDetail.ID for the records that match our current record 'matching' criteria
                    using (SqlConnection con = new SqlConnection(conStr))
                    {
                        strSQL = "SELECT p.ID  "
                        + " FROM PredictedObservedDetails p INNER JOIN ApsimFiles a ON p.ApsimFilesID = a.ID "
                        + " WHERE a.PullRequestId = @pullRequestId "
                        + "    AND a.FileName = @filename "
                        + "    AND p.TableName = @tablename "
                        + "    AND p.PredictedTableName = @predictedTableName "
                        + "    AND p.ObservedTableName = @observedTableName "
                        + "    AND p.FieldNameUsedForMatch = @fieldNameUsedForMatch ";

                        if (poDetail.FieldName2UsedForMatch.Length > 0)
                        {
                            strSQL = strSQL + "    AND p.FieldName2UsedForMatch = @fieldName2UsedForMatch ";
                        }

                        if (poDetail.FieldName3UsedForMatch.Length > 0)
                        {
                            strSQL = strSQL + "    AND p.FieldName3UsedForMatch = @fieldName3UsedForMatch ";
                        }

                        using (SqlCommand command = new SqlCommand(strSQL, con))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@PullRequestId", acceptedApsim.PullRequestId);
                            command.Parameters.AddWithValue("@filename", acceptedApsim.FileName);
                            command.Parameters.AddWithValue("@tablename", poDetail.DatabaseTableName);
                            command.Parameters.AddWithValue("@predictedTableName", poDetail.PredictedTableName);
                            command.Parameters.AddWithValue("@observedTableName", poDetail.ObservedTableName);
                            command.Parameters.AddWithValue("@fieldNameUsedForMatch", poDetail.FieldNameUsedForMatch);


                            if (poDetail.FieldName2UsedForMatch.Length > 0)
                            {
                                command.Parameters.AddWithValue("@fieldName2UsedForMatch", poDetail.FieldName2UsedForMatch);
                            }

                            if (poDetail.FieldName3UsedForMatch.Length > 0)
                            {
                                command.Parameters.AddWithValue("@fieldName3UsedForMatch", poDetail.FieldName3UsedForMatch);
                            }
                            con.Open();
                            object obj = command.ExecuteScalar();
                            con.Close();

                            if (obj != null)
                            {
                                acceptedPredictedObservedDetailsID = int.Parse(obj.ToString());
                            }
                        }
                    }

                    if (acceptedPredictedObservedDetailsID > 0)
                    {
                        //Now retreieve the matching tests data for our predicted observed details
                        using (SqlConnection con = new SqlConnection(conStr))
                        {
                            strSQL = "SELECT Variable, Test, [Current] as 'Accepted', ID As 'AcceptedPredictedObservedTestsID' "
                                   + " FROM PredictedObservedTests "
                                   + " WHERE PredictedObservedDetailsID = @PredictedObservedDetailsID ";

                            using (SqlCommand command = new SqlCommand(strSQL, con))
                            {
                                command.CommandType = CommandType.Text;
                                command.Parameters.AddWithValue("@PredictedObservedDetailsID", acceptedPredictedObservedDetailsID);

                                con.Open();
                                SqlDataReader reader = command.ExecuteReader();
                                acceptedStats = new DataTable();
                                acceptedStats.Load(reader);
                                con.Close();
                            }
                        }
                    }
                }
                return acceptedStats;
            }
            catch (Exception)
            {
                throw;
            }

        }

        private static void DeleteByPullRequest(string connectStr, int pullRequestId)
        {
            using (SqlConnection con = new SqlConnection(connectStr))
            {
                using (SqlCommand command = new SqlCommand("usp_DeleteByPullRequestId", con))
                {
                    // Configure the command and parameter.
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@PullRequestID", pullRequestId);
                    con.Open();
                    command.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        private static void DeleteByPullRequestRunDate(string connectStr, int pullRequestId, DateTime runDate)
        {
            using (SqlConnection con = new SqlConnection(connectStr))
            {
                using (SqlCommand command = new SqlCommand("usp_DeleteByPullRequestIdRunDate", con))
                {
                    // Configure the command and parameter.
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@PullRequestID", pullRequestId);
                    command.Parameters.AddWithValue("@RunDate", runDate);
                    con.Open();
                    command.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
    }
}
