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


namespace APSIM.PerformanceTests.Service
{
    public class DBFunctions
    {
        /// <summary>
        /// Gets the PredictedObservedDetail.ID for the records that match our current record 'matching' criteria
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="acceptedPullRequestID"></param>
        /// <param name="currentApsimFileFileName"></param>
        /// <param name="currentPODetails"></param>
        /// <returns></returns>
        public static int GetAcceptedPredictedObservedDetailsId(string connectStr, int acceptedPullRequestID, string currentApsimFileFileName, PredictedObservedDetails currentPODetails)
        {
            int acceptedPredictedObservedDetailsID = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    string strSQL = "SELECT p.ID  "
                    + " FROM PredictedObservedDetails p INNER JOIN ApsimFiles a ON p.ApsimFilesID = a.ID "
                    + " WHERE a.PullRequestId = @pullRequestId "
                    + "    AND a.FileName = @filename "
                    + "    AND p.TableName = @tablename "
                    + "    AND p.PredictedTableName = @predictedTableName "
                    + "    AND p.ObservedTableName = @observedTableName "
                    + "    AND p.FieldNameUsedForMatch = @fieldNameUsedForMatch ";

                    if (currentPODetails.FieldName2UsedForMatch != null)
                    {
                        if (currentPODetails.FieldName2UsedForMatch.Length > 0)
                        {
                            strSQL = strSQL + "    AND p.FieldName2UsedForMatch = @fieldName2UsedForMatch ";
                        }
                    }

                    if (currentPODetails.FieldName3UsedForMatch != null)
                    {
                        if (currentPODetails.FieldName3UsedForMatch.Length > 0)
                        {
                            strSQL = strSQL + "    AND p.FieldName3UsedForMatch = @fieldName3UsedForMatch ";
                        }
                    }

                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", acceptedPullRequestID);
                        command.Parameters.AddWithValue("@filename", currentApsimFileFileName);
                        command.Parameters.AddWithValue("@tablename", currentPODetails.DatabaseTableName);
                        command.Parameters.AddWithValue("@predictedTableName", currentPODetails.PredictedTableName);
                        command.Parameters.AddWithValue("@observedTableName", currentPODetails.ObservedTableName);
                        command.Parameters.AddWithValue("@fieldNameUsedForMatch", currentPODetails.FieldNameUsedForMatch);

                        if (currentPODetails.FieldName2UsedForMatch != null)
                        {
                            if (currentPODetails.FieldName2UsedForMatch.Length > 0)
                            {
                                command.Parameters.AddWithValue("@fieldName2UsedForMatch", currentPODetails.FieldName2UsedForMatch);
                            }
                        }

                        if (currentPODetails.FieldName3UsedForMatch != null)
                        {
                            if (currentPODetails.FieldName3UsedForMatch.Length > 0)
                            {
                                command.Parameters.AddWithValue("@fieldName3UsedForMatch", currentPODetails.FieldName3UsedForMatch);
                            }
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
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("    ERROR: Unable to retrieve 'Accepted' PredictedObservedDetailsID for Pull Request {0}: {1}", acceptedPullRequestID, ex.Message.ToString()));
            }
            return acceptedPredictedObservedDetailsID;
        }

        /// <summary>
        /// Tets the Tests stats for the 'Accepted' PredictedObservedTests 
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="acceptedPredictedObservedDetailsID"></param>
        /// <returns></returns>
        public static DataTable getPredictedObservedTestsData(string connectStr, int acceptedPredictedObservedDetailsID)
        {
            DataTable acceptedStats = new DataTable();
            try
            {
                if (acceptedPredictedObservedDetailsID > 0)
                {
                    //Now retreieve the matching tests data for our predicted observed details
                    using (SqlConnection con = new SqlConnection(connectStr))
                    {
                        string strSQL = "SELECT Variable, Test, [Current] as 'Accepted', ID As 'AcceptedPredictedObservedTestsID' "
                               + " FROM PredictedObservedTests "
                               + " WHERE PredictedObservedDetailsID = @PredictedObservedDetailsID "
                               + " ORDER BY Variable, Test, 4";

                        using (SqlCommand command = new SqlCommand(strSQL, con))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@PredictedObservedDetailsID", acceptedPredictedObservedDetailsID);

                            con.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            acceptedStats.Load(reader);
                            con.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("    ERROR: Unable to retrieve Tests Data PredictedObserved ID {0}: {1}", acceptedPredictedObservedDetailsID, ex.Message.ToString()));
            }
            return acceptedStats;
        }

        public static DataTable getPredictedObservedValues(string connectStr, int predictedObservedID)
        {
            DataTable resultDT = new DataTable();
            try
            {
                //Now retreieve the matching tests data for our predicted observed details
                using (SqlConnection con = new SqlConnection(connectStr))
                {

                    //SELECT [ID], [ValueName], [PredictedValue], [ObservedValue]
                    //  FROM [APSIM.PerformanceTests].[dbo].[PredictedObservedValues]
                    // WHERE [PredictedObservedDetailsID] = 4928
                    //   AND ([PredictedValue] IS NOT NULL AND [ObservedValue] IS NOT NULL)
                    // ORDER BY [ValueName], [ID]

                    string strSQL = "SELECT ID, ValueName, PredictedValue, ObservedValue"
                           + " FROM PredictedObservedValues "
                           + " WHERE PredictedObservedDetailsID = @PredictedObservedDetailsID " 
                           + " ORDER BY ValueName, ID ";

                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PredictedObservedDetailsID", predictedObservedID);

                        con.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        resultDT.Load(reader);
                        con.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("    ERROR: Unable to retrieve PredictedObserved Values for ID {0}: {1}", predictedObservedID, ex.Message.ToString()));
            }
            return resultDT;
        }

        /// <summary>
        /// Adds PredictedObservedTests Data to the database
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="currentApsimFileFileName"></param>
        /// <param name="currentPODetailsID"></param>
        /// <param name="currentPODetailsDatabaseTableName"></param>
        /// <param name="dtTests"></param>
        public static void AddPredictedObservedTestsData(string connectStr, string currentApsimFileFileName, int currentPODetailsID, string currentPODetailsDatabaseTableName, DataTable dtTests)
        {
            if (dtTests.Rows.Count > 0)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectStr))
                    {
                        using (SqlCommand command = new SqlCommand("usp_PredictedObservedTestsInsert", con))
                        {
                            //Now update the database with the test results
                            // Configure the command and parameter.
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@PredictedObservedID", currentPODetailsID);

                            SqlParameter tvpParam = command.Parameters.AddWithValue("@Tests", dtTests);
                            tvpParam.SqlDbType = SqlDbType.Structured;
                            tvpParam.TypeName = "dbo.PredictedObservedTestsTableType";

                            con.Open();
                            command.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    Utilities.WriteToLogFile(string.Format("    Tests Data for {0}.{1} import completed successfully!", currentApsimFileFileName, currentPODetailsDatabaseTableName));
                }

                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(string.Format("    ERROR: Unable to save Tests Data for {0}.{1}:  {2}", currentApsimFileFileName, currentPODetailsDatabaseTableName, ex.Message.ToString()));
                }
            }

        }

        /// <summary>
        /// Updates the Accepted PredictedObservedDetails.ID for the current PredictedObservedDetails record
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="acceptedPredictedObservedDetailsID"></param>
        /// <param name="currentPODetailsID"></param>
        public static void UpdatePredictedObservedDetails(string connectStr, int acceptedPredictedObservedDetailsID, int currentPODetailsID)
        {
            //Update the accepted reference for Predicted Observed Values, so that it can be 
            if (acceptedPredictedObservedDetailsID > 0 && currentPODetailsID > 0)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectStr))
                    {
                        string strSQL = "UPDATE PredictedObservedDetails "
                               + " SET AcceptedPredictedObservedDetailsID = @AcceptedPredictedObservedDetailsID "
                               + " WHERE ID = @PredictedObservedDetailsID ";

                        using (SqlCommand command = new SqlCommand(strSQL, con))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@AcceptedPredictedObservedDetailsID", acceptedPredictedObservedDetailsID);
                            command.Parameters.AddWithValue("@PredictedObservedDetailsID", currentPODetailsID);

                            con.Open();
                            command.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    Utilities.WriteToLogFile("    Accepted PredictedObservedDetailsID added to PredictedObservedDetails.");
                }

                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(string.Format("    ERROR: Unable to update 'Accepted' PredictedObservedDetailsID {0} ON PredictedObservedDetails ID {1}: {2} ", acceptedPredictedObservedDetailsID, currentPODetailsID, ex.Message.ToString()));
                }
            }
        }

        /// <summary>
        /// Updates All 'Accepted' Pull Request ID's for all ApsimFiles with specified PullRequestId
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="currentPullRequestID"></param>
        /// <param name="acceptedPullRequestID"></param>
        public static void UpdateAllApsimFileAcceptedDetails(string connectStr, int currentPullRequestID, int acceptedPullRequestID)
        {
            if ((currentPullRequestID > 0) && (acceptedPullRequestID > 0))
            {
                try
                {
                    //Need to update our 'Current' ApsimFile with PullRequestId from our Accepted Pull RequestId
                    using (SqlConnection con = new SqlConnection(connectStr))
                    {
                        string strSQL = "UPDATE ApsimFiles "
                               + " SET AcceptedPullRequestId = @AcceptedPullRequestId "
                               + " WHERE PullRequestId = @PullRequestId ";

                        using (SqlCommand command = new SqlCommand(strSQL, con))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@AcceptedPullRequestId", acceptedPullRequestID);
                            command.Parameters.AddWithValue("@PullRequestId", currentPullRequestID);

                            con.Open();
                            command.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    //Utilities.WriteToLogFile("    Accepted ApsimFilesID added to ApsimFiles.");
                }

                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(string.Format("    ERROR: Unable to update 'Accepted' Pull Request Id to {0} ON Pull Request ID {1}: {2} ", acceptedPullRequestID, currentPullRequestID, ex.Message.ToString()));
                }
            }
        }

        /// <summary>
        /// Updates 'Accepted' Pull Request ID' for ApsimFiles with current ApsimFile
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="currentApsimFileID"></param>
        /// <param name="acceptedPullRequestID"></param>
        public static void UpdateApsimFileAcceptedDetails(string connectStr, int currentApsimFileID, int acceptedPullRequestID)
        {
            try
            {
                if ((currentApsimFileID > 0) && (acceptedPullRequestID > 0))
                {
                    //Need to update our 'Current' ApsimFile with PullRequestId from our Accepted Pull RequestId
                    using (SqlConnection con = new SqlConnection(connectStr))
                    {
                        string strSQL = "UPDATE ApsimFiles "
                               + " SET AcceptedPullRequestId = @AcceptedPullRequestId "
                               + " WHERE ID = @ID ";

                        using (SqlCommand command = new SqlCommand(strSQL, con))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@AcceptedPullRequestId", acceptedPullRequestID);
                            command.Parameters.AddWithValue("@ID", currentApsimFileID);

                            con.Open();
                            command.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    //Utilities.WriteToLogFile("    Accepted ApsimFilesID added to ApsimFiles.");
                }
            }

            catch (Exception ex)
            {
                Utilities.WriteToLogFile(String.Format("    ERROR: Unable to update Accepted Pull Request ID {0} ON ApsimFile ID {1}: {2}", acceptedPullRequestID, currentApsimFileID, ex.Message.ToString()));
            }
        }

        /// <summary>
        /// UPdate the AcceptStats Log file
        /// </summary>
        /// <param name="acceptLog"></param>
        public static void UpdateAsStatsAccepted(string StatsType, AcceptStatsLog acceptLog)
        {
            try
            {
                string connectStr = Utilities.GetConnectionString();
                Utilities.WriteToLogFile("-----------------------------------");

                //make sure this is 0 if we are updating as 'Accepted'
                if (StatsType == "Accept")
                {
                    acceptLog.StatsPullRequestId = 0;
                }

                //need to authenticate the process
                int statsAccepted = Convert.ToInt32(acceptLog.LogStatus);
                using (SqlConnection con = new SqlConnection(connectStr))
                {

                    string strSQL = "INSERT INTO AcceptStatsLogs (PullRequestId, SubmitPerson, SubmitDate, LogPerson, LogReason, LogStatus, LogAcceptDate, StatsPullRequestId) "
                                  + " Values ( @PullRequestId, @SubmitPerson, @SubmitDate, @LogPerson, @LogReason, @LogStatus, @LogAcceptDate, @StatsPullRequestId )";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", acceptLog.PullRequestId);
                        command.Parameters.AddWithValue("@SubmitPerson", acceptLog.SubmitPerson);
                        command.Parameters.AddWithValue("@SubmitDate", acceptLog.SubmitDate);
                        command.Parameters.AddWithValue("@LogPerson", acceptLog.LogPerson);
                        command.Parameters.AddWithValue("@LogReason", acceptLog.LogReason);
                        command.Parameters.AddWithValue("@LogStatus", acceptLog.LogStatus);
                        command.Parameters.AddWithValue("@LogAcceptDate", acceptLog.LogAcceptDate);
                        command.Parameters.AddWithValue("@StatsPullRequestId", acceptLog.StatsPullRequestId);

                        con.Open();
                        command.ExecuteNonQuery();
                        con.Close();
                    }

                    if (StatsType == "Accept")
                    {
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