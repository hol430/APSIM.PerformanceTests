using APSIM.PerformanceTests.Models;
using APSIM.Shared.Utilities;
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
    public class UpdateStatsController : ApiController
    {

        /// <summary>
        /// Updates the Tests stats for a specified pull Request to a new pull Request Id using details passed in the Accept Log
        /// </summary>
        /// <param name="currentPullRequestID"></param>
        /// <param name="acceptedPullRequestID"></param>
        /// <returns></returns>
        [ResponseType(typeof(AcceptStatsLog))]
        public async Task<IHttpActionResult> PostStatsUpdateforPullRequest(AcceptStatsLog acceptLog)
        {
            int currentPullRequestID = acceptLog.PullRequestId;
            int acceptedPullRequestID = acceptLog.StatsPullRequestId;
            try
            {

                Utilities.WriteToLogFile("  ");
                Utilities.WriteToLogFile("==========================================================");
                Utilities.WriteToLogFile(string.Format("Updating Pull Request ID: {0}, to compare stats FROM Pull Request ID: {1}!", currentPullRequestID, acceptedPullRequestID));

                string authenCode = Utilities.GetStatsAcceptedToken();
                if (acceptLog.LogPerson == authenCode)
                {
                    DBFunctions.UpdateAsStatsAccepted("Update", acceptLog);
                    UpdateAcceptedStatsforPullRequest(currentPullRequestID, acceptedPullRequestID);
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Pull Request Id {0}, unable to update Accepted Stats from Pull Request{1}: {2}.", currentPullRequestID.ToString(), acceptedPullRequestID.ToString(), ex.Message.ToString())); ;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }


        /// <summary>
        /// Updates the Tests stats for a specified pull Request to a new pull Request Id
        /// eg:  http://localhost:53187/api/updateStats/1986/1977
        ///      http://www.apsim.info/APSIM.PerformanceTests.Service/api/updateStats/1986/1977
        /// </summary>
        /// <param name="currentPullRequestID"></param>
        /// <param name="acceptedPullRequestID"></param>
        /// <returns></returns>
        //[HttpGet]
        [Route("api/updatestats/{currentPullRequestID}/{acceptedPullRequestID}")]
        public IHttpActionResult GetUpdatedStatsforPullRequest(int currentPullRequestID, int acceptedPullRequestID)
        {
            UpdateAcceptedStatsforPullRequest(currentPullRequestID, acceptedPullRequestID);
            return StatusCode(HttpStatusCode.NoContent);
        }




        /// <summary>
        /// This will Update the 'Accepted' Stats details and referenced id's from one Pull Request ID to another Pull Request ID.
        /// </summary>
        /// <param name="currentPullRequestID"></param>
        /// <param name="acceptedPullRequestID"></param>
        private void UpdateAcceptedStatsforPullRequest(int currentPullRequestID, int acceptedPullRequestID)
        {
            //Do I need to update the log table
            string HelperMessage = string.Empty;
            try
            {
                string connectStr = Utilities.GetConnectionString();
                List<ApsimFile> currentApsimFiles = GetApsimFilesRelatedPredictedObservedData(connectStr, currentPullRequestID);

                foreach (ApsimFile currentApsimFile in currentApsimFiles)
                {
                    foreach (PredictedObservedDetails currentPODetails in currentApsimFile.PredictedObserved)
                    {
                        int acceptedPredictedObservedDetailsID = DBFunctions.GetAcceptedPredictedObservedDetailsId(connectStr, acceptedPullRequestID, currentApsimFile.FileName, currentPODetails);
                        if (acceptedPredictedObservedDetailsID > 0)
                        {
                            HelperMessage = string.Format("Current Pull Request Id: {0} to Accepted Pull Request Id: {1} for FileName: {2} - PO TableName: {3}, Current PO Id: {4}, Accepted PO Id: {5}.", currentPullRequestID, acceptedPullRequestID, currentApsimFile.FileName, currentPODetails.DatabaseTableName, currentPODetails.ID, acceptedPredictedObservedDetailsID);

                            DataTable currentPOValues = DBFunctions.getPredictedObservedValues(connectStr, currentPODetails.ID);
                            DataTable currentStats = Tests.CalculateStatsOnPredictedObservedValues(currentPOValues);

                            DataTable acceptedPOValues = DBFunctions.getPredictedObservedValues(connectStr, acceptedPredictedObservedDetailsID);
                            DataTable acceptedStats = Tests.CalculateStatsOnPredictedObservedValues(acceptedPOValues);

                            //DataTable acceptedStats = DBFunctions.getPredictedObservedTestsData(connectStr, acceptedPredictedObservedDetailsID);

                            DataTable dtTests = Tests.MergeTestsStatsAndCompare(currentStats, acceptedStats);
                            DBFunctions.AddPredictedObservedTestsData(connectStr, currentApsimFile.FileName, currentPODetails.ID, currentPODetails.DatabaseTableName, dtTests);

                            //Update the accepted reference for Predicted Observed Values, so that it can be 
                            DBFunctions.UpdatePredictedObservedDetails(connectStr, acceptedPredictedObservedDetailsID, currentPODetails.ID);
                        }
                    }
                }
                DBFunctions.UpdateAllApsimFileAcceptedDetails(connectStr, currentPullRequestID, acceptedPullRequestID);
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Unable to update {0}: {2}", HelperMessage, ex.Message.ToString())); ;
            }
        }


        /// <summary>
        /// Retrieves the ApsimFile details and related child Predicted Observed Details for a specific Pull Request
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="pullRequestId"></param>
        /// <returns></returns>
        private List<ApsimFile> GetApsimFilesRelatedPredictedObservedData(string connectStr, int pullRequestId)
        {
            string strSQL;
            List<ApsimFile> apsimFilesList = new List<ApsimFile>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    strSQL = "SELECT * FROM ApsimFiles WHERE PullRequestId = @PullRequestId ORDER BY RunDate DESC";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PullRequestId", pullRequestId);
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
                            apsim.StatsAccepted = reader.GetBoolean(5);
                            apsim.IsMerged = reader.GetBoolean(6);
                            apsim.SubmitDetails = reader.GetString(7);
                            if (reader.IsDBNull(8))
                            {
                                apsim.AcceptedPullRequestId = 0;
                            }
                            else
                            {
                                apsim.AcceptedPullRequestId = reader.GetInt32(8);
                            }
                            apsimFilesList.Add(apsim);
                        }
                        con.Close();
                    }
                }

                foreach (ApsimFile currentApsimFile in apsimFilesList)
                {
                    List<PredictedObservedDetails> currentPredictedObservedDetails = new List<PredictedObservedDetails>();
                    //retrieve the predicted observed details for this apsim file
                    using (SqlConnection con = new SqlConnection(connectStr))
                    {
                        strSQL = "SELECT * FROM PredictedObservedDetails WHERE ApsimFilesId = @ApsimFilesId ORDER BY ID";
                        using (SqlCommand command = new SqlCommand(strSQL, con))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@ApsimFilesId", currentApsimFile.ID);
                            con.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                PredictedObservedDetails predictedObserved = new PredictedObservedDetails();
                                predictedObserved.ID = reader.GetInt32(0);
                                predictedObserved.ApsimID = reader.GetInt32(1);
                                predictedObserved.DatabaseTableName = reader.GetString(2);
                                predictedObserved.PredictedTableName = reader.GetString(3);
                                predictedObserved.ObservedTableName = reader.GetString(4);
                                predictedObserved.FieldNameUsedForMatch = reader.GetString(5);
                                predictedObserved.FieldName2UsedForMatch = reader.GetString(6);
                                predictedObserved.FieldName3UsedForMatch = reader.GetString(7);
                                predictedObserved.PassedTests = reader.GetDouble(8);
                                predictedObserved.HasTests = reader.GetInt32(9);
                                if (reader.IsDBNull(10))
                                {
                                    predictedObserved.AcceptedPredictedObservedDetailsId = 0;
                                }
                                else
                                {
                                    predictedObserved.AcceptedPredictedObservedDetailsId = reader.GetInt32(10);
                                }
                                currentPredictedObservedDetails.Add(predictedObserved);
                            }
                            con.Close();
                        }
                    }
                    currentApsimFile.PredictedObserved = currentPredictedObservedDetails;
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Unable to retrieve Apsim Files and PredictedObservedDetails for Pull Request Id {0}: {1}", pullRequestId.ToString(), ex.Message.ToString()));
            }
            return apsimFilesList;
        }

        /// <summary>
        /// Retrieves the Predicted Observed Values for a specified PredictedObserved Id
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="predictedObservedId"></param>
        /// <returns></returns>
        private static DataTable GetPredictedObservedValues(string connectStr, int predictedObservedId)
        {
            DataTable dtResults = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(connectStr))
                {
                    string strSQL = "SELECT * FROM PredictedObservedValues WHERE PredictedObservedDetailsId = @PredictedObservedDetailsId ORDER BY ID DESC";
                    using (SqlCommand command = new SqlCommand(strSQL, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PredictedObservedDetailsId", predictedObservedId);

                        con.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        dtResults.Load(reader);
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteToLogFile(string.Format("ERROR:  Unable to retrieve Predicted Observed Values for PredictedObserved Id {0}: {1}", predictedObservedId.ToString(), ex.Message.ToString()));
            }
            return dtResults;
        }


    }
}
