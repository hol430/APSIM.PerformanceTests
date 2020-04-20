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
using Newtonsoft.Json;
using System.Data.Common;
using APSIM.PerformanceTests.Service.Extensions;

namespace APSIM.PerformanceTests.Service.Controllers
{
    public class ApsimFilesController : ApiController
    {
        /// <summary>
        /// Returns all ApsimFile details
        /// Usage:  GET (Read): api/apsimfiles/
        /// </summary>
        /// <returns></returns>
        public List<ApsimFile> GetAllApsimFiles()
        {
            using (SqlConnection connection = new SqlConnection(Utilities.GetConnectionString()))
            {
                connection.Open();
                return GetAllApsimFiles(connection);
            }
        }

        /// <summary>
        /// Returns the details of Apsim Files, based on the Pull Request ID
        ///  Usage:  GET (Read): api/apsimfiles/5 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(ApsimFile))]
        public List<ApsimFile> GetApsimFile(int id)
        {
            using (SqlConnection connection = new SqlConnection(Utilities.GetConnectionString()))
            {
                connection.Open();
                return GetApsimFiles(connection, id);
            }
        }

        /// <summary>
        /// NOTE:  This is intended for localhost use only, will not work on production server
        /// Deletes all files (ApsimFiles, PredictedObservedDetails, PredictedObservedValues, PredictedObservedTests and Simualtions) for
        /// the specified Pull Request Id
        /// Usage:  DELETE : api/apsimfiles/ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteByPullRequestId(int id)
        {
            try
            {
                Utilities.WriteToLogFile("-----------------------------------");
                using (SqlConnection sqlCon = new SqlConnection(Utilities.GetConnectionString()))
                {
                    sqlCon.Open();
                    DeleteByPullRequest(sqlCon, id);
                    Utilities.WriteToLogFile(string.Format("Pull Request Id {0}, deleted on {1}!", id.ToString(), System.DateTime.Now.ToString("dd/mm/yyyy HH:mm")));
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Unable to delete Pull Request Id {0}: {1}", id.ToString(), ex.Message.ToString()));
                return StatusCode(HttpStatusCode.NoContent);
            }
        }


        /// <summary>
        /// This takes the ApsimFile data, and provided from the APSIM.PerformanceTests.Collector and saves all of the date to the database
        /// Usage:  POST (Create): api/apsimfiles/ apsimfile
        /// </summary>
        /// <param name="apsimfile"></param>
        /// <returns></returns>
        [ResponseType(typeof(ApsimFile))]
        public async Task<IHttpActionResult> PostApsimFile(ApsimFile apsimfile)
        {
            Utilities.WriteToLogFile("  ");
            Utilities.WriteToLogFile("==========================================================");
            Utilities.WriteToLogFile("Post Apsim File:  Ready to process apsimfile.");
            string ErrMessageHelper = "";

            try
            {
                // fixme - dont' want an out parameter here, but
                // don't want to change anything until we have tests.
                int ApsimID;
                using (SqlConnection conn = new SqlConnection(Utilities.GetConnectionString()))
                    InsertApsimFile(conn, apsimfile, out ErrMessageHelper, out ApsimID);

                return CreatedAtRoute("DefaultApi", new { id = ApsimID }, apsimfile);
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("    ERROR in PostApsimFile:  {0} - {1}", ErrMessageHelper, ex.Message));
                throw new Exception(string.Format("    ERROR in PostApsimFile:  {0} - {1}", ErrMessageHelper, ex.Message));
            }
        }

        /// <summary>
        /// Add an apsim file to the database.
        /// </summary>
        public static void InsertApsimFile(DbConnection connection, ApsimFile apsimfile, out string ErrMessageHelper, out int ApsimID)
        {
            ApsimID = 0;
            string strSQL;

            Utilities.WriteToLogFile(string.Format("Processing PullRequestId {0}, Apsim Filename {1}, dated {2}!", apsimfile.PullRequestId, apsimfile.FileName, apsimfile.RunDate.ToString("dd/MM/yyyy HH:mm")));

            //--------------------------------------------------------------------------------------
            //Need to check if this Pull Request Id has already been used,  if it has, then we need
            //to delete everything associated with it before we save the new set of data
            //--------------------------------------------------------------------------------------
            int pullRequestCount = 0;
            Utilities.WriteToLogFile("    Checking for existing Pull Requests Details.");

            try
            {
                strSQL = "SELECT COUNT(ID) FROM ApsimFiles WHERE PullRequestId = @PullRequestId AND RunDate != @RunDate";
                using (DbCommand commandES = connection.CreateCommand())
                {
                    commandES.CommandText = strSQL;
                    commandES.CommandType = CommandType.Text;
                    commandES.AddParamWithValue("@PullRequestId", apsimfile.PullRequestId);
                    commandES.AddParamWithValue("@RunDate", apsimfile.RunDate);

                    long x = (long)commandES.ExecuteScalar();
                    pullRequestCount = (int)x;
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile("    ERROR:  Checking for existing Pull Requests: " + ex.Message.ToString());
                throw;
            }

            if (pullRequestCount > 0)
            {
                try
                {
                    Utilities.WriteToLogFile("    Removing existing Pull Requests Details.");
                    using (DbCommand commandENQ = connection.CreateCommand())
                    {
                        // Configure the command and parameter.
                        commandENQ.CommandText = ReflectionUtilities.GetResourceAsString("APSIM.PerformanceTests.Service.DeleteByPRIDNotByDate.sql");
                        commandENQ.CommandTimeout = 0;
                        commandENQ.AddParamWithValue("@PullRequestID", apsimfile.PullRequestId);
                        commandENQ.AddParamWithValue("@RunDate", apsimfile.RunDate);

                        commandENQ.ExecuteNonQuery();
                    }
                    Utilities.WriteToLogFile("    Removed original Pull Request Data.");
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile("    ERROR:  Error Removing original Pull Request Data: " + ex.Message.ToString());
                    throw;
                }
            }

            //--------------------------------------------------------------------------------------
            //Add the ApsimFile Record first, so that we can get back the IDENTITY (ID) value
            //--------------------------------------------------------------------------------------
            //using (SqlConnection con = new SqlConnection(connectStr))
            //{
            Utilities.WriteToLogFile("    Inserting ApsimFiles details.");

            try
            {
                strSQL = "INSERT INTO ApsimFiles (PullRequestId, FileName, FullFileName, RunDate, StatsAccepted, IsMerged, SubmitDetails, AcceptedPullRequestId, AcceptedRunDate) "
                       + "VALUES (@PullRequestId, @FileName, @FullFileName, @RunDate, @StatsAccepted, @IsMerged, @SubmitDetails, @AcceptedPullRequestId, @AcceptedRunDate); "
                       + Utilities.Limit(connection, "SELECT ID FROM ApsimFiles ORDER BY ID DESC", 1);
                using (DbCommand commandES = connection.CreateCommand())
                {
                    commandES.CommandText = strSQL;
                    commandES.CommandType = CommandType.Text;
                    commandES.AddParamWithValue("@PullRequestId", apsimfile.PullRequestId);
                    commandES.AddParamWithValue("@FileName", apsimfile.FileName);
                    commandES.AddParamWithValue("@FullFileName", Utilities.GetModifiedFileName(apsimfile.FullFileName));
                    commandES.AddParamWithValue("@RunDate", apsimfile.RunDate);
                    commandES.AddParamWithValue("@StatsAccepted", apsimfile.StatsAccepted);
                    commandES.AddParamWithValue("@IsMerged", apsimfile.IsMerged);
                    commandES.AddParamWithValue("@SubmitDetails", apsimfile.SubmitDetails);

                    // The accepted stats data will be set below, after we've inserted all data.
                    // In the long run, this behaviour should probably be changed.
                    commandES.AddParamWithValue("@AcceptedPullRequestId", -1);
                    commandES.AddParamWithValue("@AcceptedRunDate", "");

                    //this should return the IDENTITY value for this record (which is required for the next update)
                    ErrMessageHelper = "Filename: " + apsimfile.FileName;

                    long res = (long)commandES.ExecuteScalar();
                    ApsimID = (int)res;
                    apsimfile.ID = ApsimID;
                    ErrMessageHelper = "Filename: " + apsimfile.FileName + "- ApsimID: " + ApsimID;
                    Utilities.WriteToLogFile(string.Format("    Filename {0} inserted into ApsimFiles successfully!", apsimfile.FileName));
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile("    ERROR:  Inserting into ApsimFiles failed: " + ex.Message.ToString());
                throw;
            }

            //--------------------------------------------------------------------------------------
            //Add the Simulation Details to the database 
            //--------------------------------------------------------------------------------------
            if (apsimfile.Simulations.Rows.Count > 0)
            {
                try
                {
                    Utilities.WriteToLogFile("    Inserting Simulation details for " + apsimfile.FileName);
                    using (DbCommand commandENQ = connection.CreateCommand())
                    {
                        commandENQ.CommandText = @"INSERT INTO Simulations (ApsimFilesID, Name, OriginalSimulationID) "
                                                + "VALUES(@ApsimFilesID, @Name, @OriginalSimulationID)";
                        DbParameter param = commandENQ.CreateParameter();
                        param.ParameterName = "@ApsimFilesID";
                        commandENQ.Parameters.Add(param);

                        param = commandENQ.CreateParameter();
                        param.ParameterName = "@Name";
                        commandENQ.Parameters.Add(param);

                        param = commandENQ.CreateParameter();
                        param.ParameterName = "@OriginalSimulationID";
                        commandENQ.Parameters.Add(param);

                        foreach (DataRow row in apsimfile.Simulations.Rows)
                        {
                            commandENQ.Parameters[0].Value = apsimfile.ID;
                            commandENQ.Parameters[1].Value = row["Name"];
                            commandENQ.Parameters[2].Value = row["ID"];

                            commandENQ.ExecuteNonQuery();
                        }

                        ErrMessageHelper = "- Simualtion Data for " + apsimfile.FileName;

                        Utilities.WriteToLogFile(string.Format("    Filename {0} Simulation Data imported successfully!", apsimfile.FileName));
                    }
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile("    ERROR:  usp_SimulationsInsert failed: " + ex.Message.ToString());
                    throw;
                }
            }


            //--------------------------------------------------------------------------------------
            //Add the Predicted Observed Details (metadata) and then the data
            //--------------------------------------------------------------------------------------

            // Look at each individual set of data
            int predictedObservedID = 0;
            foreach (PredictedObservedDetails poDetail in apsimfile.PredictedObserved)
            {
                Utilities.WriteToLogFile(string.Format("    Inserting Filename {0} PredictedObserved Table Details {1}.", apsimfile.FileName, poDetail.DatabaseTableName));
                try
                {
                    strSQL = "INSERT INTO PredictedObservedDetails ("
                    + "ApsimFilesID, TableName, PredictedTableName, ObservedTableName, FieldNameUsedForMatch, FieldName2UsedForMatch, FieldName3UsedForMatch, HasTests) "
                    + "VALUES (@ApsimFilesID, @TableName, @PredictedTableName, @ObservedTableName, @FieldNameUsedForMatch, @FieldName2UsedForMatch, @FieldName3UsedForMatch, 1); "
                    + "SELECT ID FROM PredictedObservedDetails ORDER BY ID DESC LIMIT 1;";

                    using (DbCommand commandES = connection.CreateCommand())
                    {
                        commandES.CommandText = strSQL;
                        commandES.CommandType = CommandType.Text;
                        commandES.AddParamWithValue("@ApsimFilesID", ApsimID);
                        commandES.AddParamWithValue("@TableName", poDetail.DatabaseTableName);
                        commandES.AddParamWithValue("@PredictedTableName", poDetail.PredictedTableName);
                        commandES.AddParamWithValue("@ObservedTableName", poDetail.ObservedTableName);

                        commandES.AddParamWithValue("@FieldNameUsedForMatch", poDetail.FieldNameUsedForMatch);
                        commandES.AddParamWithValue("@FieldName2UsedForMatch", poDetail.FieldName2UsedForMatch);
                        commandES.AddParamWithValue("@FieldName3UsedForMatch", poDetail.FieldName3UsedForMatch);

                        //this should return the IDENTITY value for this record (which is required for the next update)
                        ErrMessageHelper = "PredictedObservedDetails for " + poDetail.DatabaseTableName;

                        long res = (long)commandES.ExecuteScalar();
                        predictedObservedID = (int)res;
                        ErrMessageHelper = "PredictedObservedDetails for " + poDetail.DatabaseTableName + "(ID: " + predictedObservedID + ")";
                        Utilities.WriteToLogFile(string.Format("    Filename {0} PredictedObserved Table Details {1}, (Id: {2}) imported successfully!", apsimfile.FileName, poDetail.DatabaseTableName, predictedObservedID));
                    }
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile("    ERROR:  INSERT INTO PredictedObservedDetails failed: " + ex.Message.ToString());
                    throw;
                }


                //--------------------------------------------------------------------------------------
                //And finally this is where we will insert the actual Predicted Observed DATA
                //--------------------------------------------------------------------------------------
                Utilities.WriteToLogFile(string.Format("    PredictedObserved Data Values for {0}.{1} - import started!", apsimfile.FileName, poDetail.DatabaseTableName));

                //need to find the first (and then each instance thereafter) of a field name being with Observed,
                //the get the corresponding Predicted field name, and then create a new table definition based on this
                //data,
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO PredictedObservedValues "
                                        + "(PredictedObservedDetailsID, SimulationsID, MatchName, MatchValue, MatchName2, MatchValue2, MatchName3, MatchValue3, ValueName, PredictedValue, ObservedValue) "
                                        + "VALUES(@PredictedObservedDetailsID, @SimulationsID, @MatchName, @MatchValue, @MatchName2, @MatchValue2, @MatchName3, @MatchValue3, @ValueName, @PredictedValue, @ObservedValue)";

                    command.AddParamWithValue("@PredictedObservedDetailsID", predictedObservedID);
                    command.AddParamWithValue("@MatchName", poDetail.FieldNameUsedForMatch);
                    command.AddParamWithValue("MatchName2", poDetail.FieldName2UsedForMatch);
                    command.AddParamWithValue("MatchName3", poDetail.FieldName3UsedForMatch);

                    command.AddParamWithValue("@SimulationsID", 0);
                    command.AddParamWithValue("@MatchValue", null);
                    command.AddParamWithValue("@MatchValue2", null);
                    command.AddParamWithValue("@MatchValue3", null);

                    command.AddParamWithValue("@ValueName", "");
                    command.AddParamWithValue("@PredictedValue", 0d);
                    command.AddParamWithValue("@ObservedValue", 0d);

                    for (int i = 0; i < poDetail.Data.Columns.Count; i++)
                    {
                        string observedColumnName = poDetail.Data.Columns[i].ColumnName.Trim();
                        if (observedColumnName.StartsWith("Observed."))
                        {
                            // Get the corresponding predicted column name.
                            string valueName = observedColumnName.Replace("Observed.", "");
                            string predictedColumnName = $"Predicted.{valueName}";
                            command.Parameters["@ValueName"].Value = valueName;

                            foreach (DataRow row in poDetail.Data.Rows)
                            {
                                int simulationsID = GetSimulationID(connection, ApsimID, (int)row["SimulationID"]);
                                command.Parameters["@SimulationsID"].Value = simulationsID;
                                command.Parameters["@MatchValue"].Value = row[poDetail.FieldNameUsedForMatch];

                                // MatchValue2 and 3 are already set to null.
                                if (!string.IsNullOrEmpty(poDetail.FieldName2UsedForMatch))
                                    command.Parameters["@MatchValue2"].Value = row[poDetail.FieldName2UsedForMatch];

                                if (!string.IsNullOrEmpty(poDetail.FieldName3UsedForMatch))
                                    command.Parameters["@MatchValue3"].Value = row[poDetail.FieldName3UsedForMatch];

                                command.Parameters["@PredictedValue"].Value = row[predictedColumnName];
                                command.Parameters["@ObservedValue"].Value = row[observedColumnName];

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                //Need to run the testing procecedure here, and then save the test data
                if (poDetail.Data.Rows.Count > 0)
                {
                    ErrMessageHelper = string.Empty;

                    Utilities.WriteToLogFile(string.Format("    Tests Data for {0}.{1} import started.", apsimfile.FileName, poDetail.DatabaseTableName));

                    //need to retrieve data for the "AcceptedStats" version, so that we can update the stats
                    int acceptedPredictedObservedDetailsID = 0;    //this should get updated in 'RetrieveAcceptedStatsData' 
                    ErrMessageHelper = "Processing RetrieveAcceptedStatsData.";
                    DataTable acceptedStats = RetrieveAcceptedStatsData(connection, ApsimID, apsimfile, poDetail, predictedObservedID, ref acceptedPredictedObservedDetailsID);

                    ErrMessageHelper = "Processing Tests.DoValidationTest.";
                    DataTable dtTests = Tests.DoValidationTest(poDetail.DatabaseTableName, poDetail.Data, acceptedStats);

                    ErrMessageHelper = "Processing DBFunctions.AddPredictedObservedTestsData.";
                    DBFunctions.AddPredictedObservedTestsData(connection, apsimfile.FileName, predictedObservedID, poDetail.DatabaseTableName, dtTests);

                    //Update the accepted reference for Predicted Observed Values, so that it can be 
                    ErrMessageHelper = "Processing DBFunctions.UpdatePredictedObservedDetails.";
                    DBFunctions.UpdatePredictedObservedDetails(connection, acceptedPredictedObservedDetailsID, predictedObservedID);
                }
            }
        }

        /// <summary>
        /// Gets the ID of the record in the simulations table with a given ApsimFile ID and SimulationID.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="apsimFileID">ID of the apsim file.</param>
        /// <param name="simulationID">ID of the simulation in the _Simulations table in the apsim .db file.</param>
        private static int GetSimulationID(DbConnection connection, int apsimFileID, int simulationID)
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT ID FROM Simulations WHERE ApsimFilesID = @ApsimFileID AND OriginalSimulationID = @SimulationID";
                command.AddParamWithValue("@ApsimFileID", apsimFileID);
                command.AddParamWithValue("@SimulationID", simulationID);

                long res = (long)command.ExecuteScalar();
                return (int)res;
            }
        }

        /// <summary>
        /// Returns the PredictedObservedTests data for 'Accepted' data set, based on matching 'Current' Details
        /// </summary>
        /// <param name="conStr"></param>
        /// <param name="currentApsimID"></param>
        /// <param name="currentApsim"></param>
        /// <param name="poDetail"></param>
        /// <param name="predictedObservedId"></param>
        /// <param name="acceptedPredictedObservedDetailsID"></param>
        /// <returns></returns>
        private static DataTable RetrieveAcceptedStatsData(DbConnection connection, int currentApsimID, ApsimFile currentApsim, PredictedObservedDetails poDetail, int predictedObservedId, ref int acceptedPredictedObservedDetailsID)
        {
            DataTable acceptedStats = new DataTable();
            ApsimFile acceptedApsim = new ApsimFile();
            try
            {
                string strSQL = "SELECT * FROM ApsimFiles WHERE StatsAccepted = 1 AND PullRequestId != @PullRequestId ORDER BY RunDate DESC";
                strSQL = Utilities.Limit(connection, strSQL, 1);
                using (DbCommand command = connection.CreateCommand(strSQL))
                {
                    command.CommandType = CommandType.Text;
                    command.AddParamWithValue("@PullRequestId", currentApsim.PullRequestId);

                    using (DbDataReader sdReader = command.ExecuteReader())
                    {
                        while (sdReader.Read())
                        {
                            acceptedApsim.ID = sdReader.GetInt32(0);
                            acceptedApsim.PullRequestId = sdReader.GetInt32(1);
                            acceptedApsim.FileName = sdReader.GetString(2);
                            acceptedApsim.FullFileName = sdReader.GetString(3);
                            acceptedApsim.RunDate = sdReader.GetDateTime(4);
                            acceptedApsim.StatsAccepted = sdReader.GetBoolean(5);
                            acceptedApsim.IsMerged = sdReader.GetBoolean(6);
                            acceptedApsim.SubmitDetails = sdReader.GetString(7);
                            acceptedApsim.AcceptedPullRequestId = sdReader.IsDBNull(8) ? 0 : sdReader.GetInt32(8);
                        }
                    }
                }

                if (acceptedApsim.PullRequestId > 0)
                {
                    DBFunctions.UpdateApsimFileAcceptedDetails(connection, currentApsim.PullRequestId, acceptedApsim.PullRequestId, acceptedApsim.RunDate);

                    ////get the PredictedObservedDetail.ID for the records that match our current record 'matching' criteria
                    acceptedPredictedObservedDetailsID = DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, acceptedApsim.PullRequestId, currentApsim.FileName, poDetail);
                    ////Now retreieve the matching tests data for our predicted observed details
                    acceptedStats = DBFunctions.GetPredictedObservedTestsData(connection, acceptedPredictedObservedDetailsID);
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("    ERROR:  Unable to RetrieveAcceptedStatsData for ApsimFile {0}: Pull Request Id {1}: {2}.", currentApsim.FileName, currentApsim.PullRequestId, ex.Message.ToString()));
            }
            return acceptedStats;
        }

        /// <summary>
        /// Deletes all Data for a specified Pull RequestId
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="pullRequestId"></param>
        public static void DeleteByPullRequest(DbConnection sqlCon, int pullRequestId)
        {
            string sql = ReflectionUtilities.GetResourceAsString("APSIM.PerformanceTests.Service.DeleteByPullRequest.sql");
            using (DbCommand command = sqlCon.CreateCommand(sql))
            {
                // Configure the command and parameter.
                command.CommandTimeout = 0;
                command.AddParamWithValue("@PullRequestID", pullRequestId);

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes all Data for a specified Pull RequestId, excluding those for this Run Date
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="pullRequestId"></param>
        /// <param name="runDate"></param>
        private static void DeleteByPullRequestButNotRunDate(SqlConnection sqlCon, int pullRequestId, DateTime runDate)
        {
            try
            {
                using (SqlCommand commandENQ = new SqlCommand("usp_DeleteByPullRequestIdButNotRunDate", sqlCon))
                {

                    // Configure the command and parameter.
                    commandENQ.CommandType = CommandType.StoredProcedure;
                    commandENQ.CommandTimeout = 0;
                    commandENQ.Parameters.AddWithValue("@PullRequestID", pullRequestId);
                    commandENQ.Parameters.AddWithValue("@RunDate", runDate);

                    commandENQ.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("    ERROR:  Unable to remove data for Pull Request Id: {0} on {1}: {2}.", pullRequestId, runDate.ToString("dd/MM/yyyy HH:mm"), ex.Message.ToString()));
            }
        }

        /// <summary>
        /// todo: should this be internal?
        /// </summary>
        /// <param name="connection"></param>
        public static List<ApsimFile> GetAllApsimFiles(DbConnection connection)
        {
            List<ApsimFile> apsimFiles = new List<ApsimFile>();

            string strSQL = "SELECT * FROM ApsimFiles ORDER BY RunDate DESC, PullRequestId;";
            using (DbCommand commandER = connection.CreateCommand(strSQL))
            {
                commandER.CommandType = CommandType.Text;
                using (DbDataReader reader = commandER.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        apsimFiles.Add(new ApsimFile
                        {
                            ID = reader.GetInt32(0),
                            PullRequestId = reader.GetInt32(1),
                            FileName = reader.GetString(2),
                            FullFileName = reader.GetString(3),
                            RunDate = reader.GetDateTime(4),
                            StatsAccepted = reader.GetBoolean(5),
                            IsMerged = reader.GetBoolean(6),
                            SubmitDetails = reader.GetString(7),
                            AcceptedPullRequestId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                        });
                    }
                }
            }

            return apsimFiles;
        }

        /// <summary>
        /// Get an apsim file with a given ID.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<ApsimFile> GetApsimFiles(DbConnection connection, int id)
        {
            List<ApsimFile> files = new List<ApsimFile>();

            string strSQL = "SELECT * FROM ApsimFiles WHERE PullRequestId = @PullRequestId ORDER BY RunDate DESC";
            using (DbCommand commandER = connection.CreateCommand(strSQL))
            {
                commandER.AddParamWithValue("@PullRequestId", id);
                using (DbDataReader reader = commandER.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ApsimFile apsim = new ApsimFile
                        {
                            ID = reader.GetInt32(0),
                            PullRequestId = reader.GetInt32(1),
                            FileName = reader.GetString(2),
                            FullFileName = reader.GetString(3),
                            RunDate = reader.GetDateTime(4),
                            StatsAccepted = reader.GetBoolean(5),
                            IsMerged = reader.GetBoolean(6),
                            SubmitDetails = reader.GetString(7)
                        };

                        if (reader.IsDBNull(8))
                        {
                            apsim.AcceptedPullRequestId = 0;
                        }
                        else
                        {
                            apsim.AcceptedPullRequestId = reader.GetInt32(8);
                        }

                        files.Add(apsim);
                    }
                    return files;
                }
            }
        }
    }
}
