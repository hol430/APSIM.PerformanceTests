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
using System.Data.Common;
using APSIM.PerformanceTests.Service.Extensions;

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
        public static int GetAcceptedPredictedObservedDetailsId(DbConnection connection, int acceptedPullRequestID, string currentApsimFileFileName, PredictedObservedDetails currentPODetails)
        {

            int acceptedPredictedObservedDetailsID = 0;
            try
            {
                string strSQL = "SELECT p.ID  "
                + " FROM PredictedObservedDetails p INNER JOIN ApsimFiles a ON p.ApsimFilesID = a.ID "
                + " WHERE a.PullRequestId = @pullRequestId "
                + "    AND a.FileName = @filename "
                + "    AND p.TableName = @tablename ";

                //modLCM - 22/02/2018 - As per instructions from Dean, remove matching on extra columns in PO Details Table

                //+ "    AND p.PredictedTableName = @predictedTableName "
                //+ "    AND p.ObservedTableName = @observedTableName "
                //+ "    AND p.FieldNameUsedForMatch = @fieldNameUsedForMatch ";

                //if ((currentPODetails.FieldName2UsedForMatch != null) &&  (currentPODetails.FieldName2UsedForMatch.Length > 0))
                //{
                //    strSQL = strSQL + "    AND p.FieldName2UsedForMatch = @fieldName2UsedForMatch ";
                //}

                //if ((currentPODetails.FieldName3UsedForMatch != null) && (currentPODetails.FieldName3UsedForMatch.Length > 0))
                //{
                //    strSQL = strSQL + "    AND p.FieldName3UsedForMatch = @fieldName3UsedForMatch ";
                //}

                using (DbCommand commandES = connection.CreateCommand(strSQL))
                {
                    commandES.CommandType = CommandType.Text;
                    commandES.AddParamWithValue("@PullRequestId", acceptedPullRequestID);
                    commandES.AddParamWithValue("@filename", currentApsimFileFileName);
                    commandES.AddParamWithValue("@tablename", currentPODetails.DatabaseTableName);
                    //commandES.Parameters.AddWithValue("@predictedTableName", currentPODetails.PredictedTableName);
                    //commandES.Parameters.AddWithValue("@observedTableName", currentPODetails.ObservedTableName);
                    //commandES.Parameters.AddWithValue("@fieldNameUsedForMatch", currentPODetails.FieldNameUsedForMatch);

                    //if ((currentPODetails.FieldName2UsedForMatch != null) && (currentPODetails.FieldName2UsedForMatch.Length > 0))
                    //{
                    //    commandES.Parameters.AddWithValue("@fieldName2UsedForMatch", currentPODetails.FieldName2UsedForMatch);
                    //}

                    //if ((currentPODetails.FieldName3UsedForMatch != null) && (currentPODetails.FieldName3UsedForMatch.Length > 0))
                    //{
                    //    commandES.Parameters.AddWithValue("@fieldName3UsedForMatch", currentPODetails.FieldName3UsedForMatch);
                    //}

                    object obj = commandES.ExecuteScalar();

                    if (obj != null)
                    {
                        acceptedPredictedObservedDetailsID = int.Parse(obj.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("    ERROR in GetAcceptedPredictedObservedDetailsId: Unable to retrieve 'Accepted' PredictedObservedDetailsID for Pull Request {0}: {1}", acceptedPullRequestID, ex.Message.ToString()));
            }
            return acceptedPredictedObservedDetailsID;
        }

 
        /// <summary>
        /// Retrieves the Tests stats for the 'Accepted' PredictedObservedTests 
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="acceptedPredictedObservedDetailsID"></param>
        /// <returns></returns>
        public static DataTable GetPredictedObservedTestsData(DbConnection connection, int acceptedPredictedObservedDetailsID)
        {
            DataTable acceptedStats = new DataTable();
            if (acceptedPredictedObservedDetailsID > 0)
            {
                try
                {
                    string strSQL = "SELECT Variable, Test, [Current] as 'Accepted', ID As 'AcceptedPredictedObservedTestsID' "
                            + " FROM PredictedObservedTests "
                            + " WHERE PredictedObservedDetailsID = @PredictedObservedDetailsID "
                            + " ORDER BY Variable, Test, 4";

                    using (DbCommand commandER = connection.CreateCommand(strSQL))
                    {
                        commandER.CommandType = CommandType.Text;
                        commandER.AddParamWithValue("@PredictedObservedDetailsID", acceptedPredictedObservedDetailsID);

                        DbDataReader reader = commandER.ExecuteReader();
                        acceptedStats.Load(reader);
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(string.Format("    ERROR in getPredictedObservedTestsData: Unable to retrieve Tests Data PredictedObserved ID {0}: {1}", acceptedPredictedObservedDetailsID, ex.Message.ToString()));
                }
            }
            return acceptedStats;
        }

        /// <summary>
        /// Retreieves the matching Predicted Pbserved Values based on the Predicted Pbserved Id
        /// </summary>
        /// <param name="sqlCon"></param>
        /// <param name="predictedObservedID"></param>
        /// <returns></returns>
        public static DataTable GetPredictedObservedValues(SqlConnection sqlCon, int predictedObservedID)
        {
            DataTable resultDT = new DataTable();
            try
            {
                string strSQL = "SELECT ID, ValueName, PredictedValue, ObservedValue"
                        + " FROM PredictedObservedValues "
                        + " WHERE PredictedObservedDetailsID = @PredictedObservedDetailsID " 
                        + " ORDER BY ValueName, ID ";

                using (SqlCommand commandER = new SqlCommand(strSQL, sqlCon))
                {
                    commandER.CommandType = CommandType.Text;
                    commandER.Parameters.AddWithValue("@PredictedObservedDetailsID", predictedObservedID);

                    SqlDataReader reader = commandER.ExecuteReader();
                    resultDT.Load(reader);
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("    ERROR in getPredictedObservedValues: Unable to retrieve PredictedObserved Values for ID {0}: {1}", predictedObservedID, ex.Message.ToString()));
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
        public static void AddPredictedObservedTestsData(DbConnection sqlCon, string currentApsimFileFileName, int currentPODetailsID, string currentPODetailsDatabaseTableName, DataTable dtTests)
        {
            if (dtTests.Rows.Count > 0)
            {
                try
                {
                    bool passedTests = true;

                    string sql = "INSERT INTO PredictedObservedTests " +
                                 "(PredictedObservedDetailsID, Variable, Test, Accepted, Current, Difference, PassedTest, AcceptedPredictedObservedTestsID, IsImprovement, SortOrder, DifferencePercent)\n" +
                                 "VALUES (@PredictedObservedDetailsID, @Variable, @Test, @Accepted, @Current, @Difference, @PassedTest, @AcceptedPredictedObservedTestsID, @IsImprovement, @SortOrder, @DifferencePercent);";
                    using (DbCommand commandENQ = sqlCon.CreateCommand(sql))
                    {
                        commandENQ.AddParamWithValue("@PredictedObservedDetailsID", currentPODetailsID);
                        commandENQ.AddParamWithValue("@Variable", "");
                        commandENQ.AddParamWithValue("@Test", "");
                        commandENQ.AddParamWithValue("@Accepted", 0d);
                        commandENQ.AddParamWithValue("@Current", 0d);
                        commandENQ.AddParamWithValue("@Difference", 0d);
                        commandENQ.AddParamWithValue("@PassedTest", 0);//int
                        commandENQ.AddParamWithValue("@AcceptedPredictedObservedTestsID", 0);//int
                        commandENQ.AddParamWithValue("@IsImprovement", 0);//int
                        commandENQ.AddParamWithValue("@SortOrder", 0);//int
                        commandENQ.AddParamWithValue("@DifferencePercent", null);

                        foreach (DataRow row in dtTests.Rows)
                        {
                            string test = (string)row["Test"];
                            int passedTest = row["PassedTest"] == DBNull.Value ? 0 : int.Parse(row["PassedTest"].ToString());
                            if ((string)row["Test"] != "Name")
                            {
                                if (passedTest == 0)
                                    passedTests = false;

                                commandENQ.Parameters["@Variable"].Value = row["Variable"];
                                commandENQ.Parameters["@Test"].Value = test;
                                commandENQ.Parameters["@Accepted"].Value = row["Accepted"];
                                commandENQ.Parameters["@Current"].Value = row["Current"];
                                commandENQ.Parameters["@Difference"].Value = row["Difference"];
                                commandENQ.Parameters["@PassedTest"].Value = row["PassedTest"];
                                commandENQ.Parameters["@AcceptedPredictedObservedTestsID"].Value = row["AcceptedPredictedObservedTestsID"];
                                commandENQ.Parameters["@IsImprovement"].Value = row["IsImprovement"];
                                commandENQ.Parameters["@SortOrder"].Value = test == "n" ? 0 : 1;

                                if (row["Accepted"] != DBNull.Value && row["Difference"] != DBNull.Value)
                                {
                                    double accepted = (double)row["Accepted"];
                                    double difference = (double)row["Difference"];

                                    double diffPercent = 0;
                                    if (accepted != 0 && difference != 0)
                                        diffPercent = 100.0 * difference / accepted;

                                    commandENQ.Parameters["@DifferencePercent"].Value = diffPercent;
                                }
                                else
                                    commandENQ.Parameters["@DifferencePercent"].Value = null;

                                commandENQ.ExecuteNonQuery();
                            }
                        }
                    }

                    // Update the PredictedObservedDetails table and set the PassedTests field appropriately.
                    sql = "UPDATE PredictedObservedDetails " +
                          "SET    PassedTests = @PassedTests, " +
                                 "HasTests    = 1 " +
                          "WHERE  ID          = @PredictedObservedID";
                    using (DbCommand command = sqlCon.CreateCommand(sql))
                    {
                        command.AddParamWithValue("@PassedTests", passedTests ? 1 : 0);
                        command.AddParamWithValue("@PredictedObservedID", currentPODetailsID);

                        command.ExecuteNonQuery();
                    }

                    Utilities.WriteToLogFile(string.Format("    Tests Data for {0}.{1} import completed successfully!", currentApsimFileFileName, currentPODetailsDatabaseTableName));
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(string.Format("    ERROR in AddPredictedObservedTestsData: Unable to save Tests Data for {0}.{1}:  {2}", currentApsimFileFileName, currentPODetailsDatabaseTableName, ex.Message.ToString()));
                }
            }
        }

        /// <summary>
        /// Updates the Accepted PredictedObservedDetails.ID for the current PredictedObservedDetails record
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="acceptedPredictedObservedDetailsID"></param>
        /// <param name="currentPODetailsID"></param>
        public static void UpdatePredictedObservedDetails(DbConnection sqlCon, int acceptedPredictedObservedDetailsID, int currentPODetailsID)
        {
            //Update the accepted reference for Predicted Observed Values, so that it can be 
            if (acceptedPredictedObservedDetailsID > 0 && currentPODetailsID > 0)
            {
                try
                {
                    string sql = "UPDATE PredictedObservedDetails "
                            + " SET AcceptedPredictedObservedDetailsID = @AcceptedPredictedObservedDetailsID "
                            + " WHERE ID = @PredictedObservedDetailsID ";

                    using (DbCommand command = sqlCon.CreateCommand(sql))
                    {
                        command.CommandType = CommandType.Text;
                        command.AddParamWithValue("@AcceptedPredictedObservedDetailsID", acceptedPredictedObservedDetailsID);
                        command.AddParamWithValue("@PredictedObservedDetailsID", currentPODetailsID);

                        command.ExecuteNonQuery();
                    }
                    Utilities.WriteToLogFile("    Accepted PredictedObservedDetailsID added to PredictedObservedDetails.");
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(string.Format("    ERROR in UpdatePredictedObservedDetails: Unable to update 'Accepted' PredictedObservedDetailsID {0} ON PredictedObservedDetails ID {1}: {2} ", acceptedPredictedObservedDetailsID, currentPODetailsID, ex.Message.ToString()));
                }
            }
        }


        /// <summary>
        /// Updates 'Accepted' ApsimFile details (PullRequestId, RunDate) for ApsimFile using Current Pull RequestId
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="currentApsimFileID"></param>
        /// <param name="acceptedPullRequestID"></param>
        public static void UpdateApsimFileAcceptedDetails(DbConnection sqlCon, int currentPullRequestID, int acceptedPullRequestID, DateTime acceptedRunDate)
        {
            if ((currentPullRequestID > 0) && (acceptedPullRequestID > 0))
            {
                try
                {
                    string strSQL = "UPDATE ApsimFiles "
                            + " SET AcceptedPullRequestId = @AcceptedPullRequestId, "
                            + " AcceptedRunDate = @AcceptedRunDate "
                            + " WHERE PullRequestId = @PullRequestID ";

                    using (DbCommand commandENQ = sqlCon.CreateCommand(strSQL))
                    {
                        commandENQ.CommandType = CommandType.Text;
                        commandENQ.AddParamWithValue("@AcceptedPullRequestId", acceptedPullRequestID);
                        commandENQ.AddParamWithValue("@AcceptedRunDate", acceptedRunDate);
                        commandENQ.AddParamWithValue("@PullRequestID", currentPullRequestID);

                        commandENQ.ExecuteNonQuery();
                    }
                    //Utilities.WriteToLogFile("    Accepted ApsimFilesID added to ApsimFiles.");
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(String.Format("    ERROR in UpdateApsimFileAcceptedDetails: Unable to update Accepted Pull Request {0} Details for Pull Request ID {1}: {2}", acceptedPullRequestID, currentPullRequestID, ex.Message.ToString()));
                }
            }
        }

        /// <summary>
        /// Update the AcceptStats Log file
        /// </summary>
        /// <param name="acceptLog"></param>
        public static void UpdateAsStatsAccepted(DbConnection connection, string StatsType, AcceptStatsLog acceptLog)
        {
            Utilities.WriteToLogFile("-----------------------------------");

            //make sure this is 0 if we are updating as 'Accepted'
            if (StatsType == "Accept")
            {
                acceptLog.StatsPullRequestId = 0;
                //acceptLog.FileCount = 0;
            }

            //need to authenticate the process
            int statsAccepted = Convert.ToInt32(acceptLog.LogStatus);

            try
            {
                string strSQL = "INSERT INTO AcceptStatsLogs (PullRequestId, SubmitPerson, SubmitDate, FileCount, LogPerson, LogReason, LogStatus, LogAcceptDate, StatsPullRequestId) "
                                + " Values ( @PullRequestId, @SubmitPerson, @SubmitDate, @FileCount, @LogPerson, @LogReason, @LogStatus, @LogAcceptDate, @StatsPullRequestId )";
                using (DbCommand commandENQ = connection.CreateCommand(strSQL))
                {
                    commandENQ.CommandType = CommandType.Text;
                    commandENQ.AddParamWithValue("@PullRequestId", acceptLog.PullRequestId);
                    commandENQ.AddParamWithValue("@SubmitPerson", acceptLog.SubmitPerson);
                    commandENQ.AddParamWithValue("@SubmitDate", acceptLog.SubmitDate);
                    commandENQ.AddParamWithValue("@FileCount", acceptLog.FileCount);
                    commandENQ.AddParamWithValue("@LogPerson", acceptLog.LogPerson);
                    commandENQ.AddParamWithValue("@LogReason", acceptLog.LogReason);
                    commandENQ.AddParamWithValue("@LogStatus", acceptLog.LogStatus);
                    commandENQ.AddParamWithValue("@LogAcceptDate", acceptLog.LogAcceptDate);
                    commandENQ.AddParamWithValue("@StatsPullRequestId", acceptLog.StatsPullRequestId);

                    commandENQ.ExecuteNonQuery();
                }

                if (StatsType == "Accept")
                {
                    strSQL = "UPDATE ApsimFiles SET StatsAccepted = @StatsAccepted, IsMerged = @IsMerged WHERE PullRequestId = @PullRequestId";
                    using (DbCommand commandENQ = connection.CreateCommand(strSQL))
                    {
                        commandENQ.CommandType = CommandType.Text;
                        commandENQ.AddParamWithValue("@StatsAccepted", statsAccepted);
                        commandENQ.AddParamWithValue("@IsMerged", statsAccepted);        //do this the same to during changeover
                        commandENQ.AddParamWithValue("@PullRequestId", acceptLog.PullRequestId);

                        commandENQ.ExecuteNonQuery();
                    }
                }
                //Utilities.WriteToLogFile(string.Format("    Accept Stats Status updated to {0} by {1} on {2}. Reason: {3}", acceptLog.LogStatus, acceptLog.LogPerson, acceptLog.SubmitDate, acceptLog.LogReason));
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR in UpdateAsStatsAccepted:  Pull Request Id {0}, Failed to update as 'Stats Accepted': {1}", acceptLog.PullRequestId.ToString(), ex.Message.ToString()));
            }
        }

        public static DateTime GetLatestPullRequestRunDate(SqlConnection sqlCon, int acceptedPullRequestID)
        {
            DateTime returnDate = new DateTime();

            string strSQL = "SELECT TOP 1 RunDate FROM ApsimFiles WHERE PullRequestId = @PullRequestId ORDER BY RunDate DESC";
            using (SqlCommand commandES = new SqlCommand(strSQL, sqlCon))
            {
                commandES.CommandType = CommandType.Text;
                commandES.Parameters.AddWithValue("@PullRequestId", acceptedPullRequestID);

                returnDate = (DateTime)commandES.ExecuteScalar();
            }
            return returnDate;
        }

    }
}