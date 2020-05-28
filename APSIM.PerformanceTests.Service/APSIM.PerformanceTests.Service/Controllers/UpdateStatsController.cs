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
                    using (SqlConnection connection = new SqlConnection(Utilities.GetConnectionString()))
                    {
                        connection.Open();
                        DBFunctions.UpdateAsStatsAccepted(connection, "Update", acceptLog);
                    }
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
        ///      http://apsimdev.apsim.info/APSIM.PerformanceTests.Service/api/updateStats/1986/1977
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
            string connectStr = Utilities.GetConnectionString();

            using (SqlConnection sqlCon = new SqlConnection(connectStr))
            {
                sqlCon.Open();
                try
                {
                    List<ApsimFile> currentApsimFiles = DBFunctions.GetApsimFilesRelatedPredictedObservedData(sqlCon, currentPullRequestID);

                    //need to get the (latest) run date for the acceptedPullRequestID 
                    DateTime acceptedRunDate = DBFunctions.GetLatestRunDateForPullRequest(sqlCon, acceptedPullRequestID);

                    foreach (ApsimFile currentApsimFile in currentApsimFiles)
                    {
                        foreach (PredictedObservedDetails currentPODetails in currentApsimFile.PredictedObserved)
                        {
                            int acceptedPredictedObservedDetailsID = DBFunctions.GetAcceptedPredictedObservedDetailsId(sqlCon, acceptedPullRequestID, currentApsimFile.FileName, currentPODetails);
                            if (acceptedPredictedObservedDetailsID > 0)
                            {
                                HelperMessage = string.Format("Current Pull Request Id: {0} to Accepted Pull Request Id: {1} for FileName: {2} - PO TableName: {3}, Current PO Id: {4}, Accepted PO Id: {5}.", currentPullRequestID, acceptedPullRequestID, currentApsimFile.FileName, currentPODetails.DatabaseTableName, currentPODetails.ID, acceptedPredictedObservedDetailsID);

                                DataTable currentPOValues = DBFunctions.GetPredictedObservedValues(sqlCon, currentPODetails.ID);
                                DataTable currentStats = Tests.CalculateStatsOnPredictedObservedValues(currentPOValues);

                                DataTable acceptedPOValues = DBFunctions.GetPredictedObservedValues(sqlCon, acceptedPredictedObservedDetailsID);
                                DataTable acceptedStats = Tests.CalculateStatsOnPredictedObservedValues(acceptedPOValues);

                                //DataTable acceptedStats = DBFunctions.getPredictedObservedTestsData(connectStr, acceptedPredictedObservedDetailsID);

                                DataTable dtTests = Tests.MergeTestsStatsAndCompare(currentStats, acceptedStats);
                                DBFunctions.AddPredictedObservedTestsData(sqlCon, currentApsimFile.FileName, currentPODetails.ID, currentPODetails.DatabaseTableName, dtTests);

                                //Update the accepted reference for Predicted Observed Values, so that it can be 
                                DBFunctions.UpdatePredictedObservedDetails(sqlCon, acceptedPredictedObservedDetailsID, currentPODetails.ID);
                            }
                        }
                    }
                    DBFunctions.UpdateApsimFileAcceptedDetails(sqlCon, currentPullRequestID, acceptedPullRequestID, acceptedRunDate);
                }
                catch (Exception ex)
                {
                    Utilities.WriteToLogFile(string.Format("ERROR:  Unable to update {0}: {1}", HelperMessage, ex.Message.ToString())); ;
                }
            }
        }

        /// <summary>
        /// Retrieves the Predicted Observed Values for a specified PredictedObserved Id
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="predictedObservedId"></param>
        /// <returns></returns>
        private static DataTable GetPredictedObservedValues(SqlConnection sqlCon, int predictedObservedId)
        {
            DataTable dtResults = new DataTable();
            try
            {
                string strSQL = "SELECT * FROM PredictedObservedValues WHERE PredictedObservedDetailsId = @PredictedObservedDetailsId ORDER BY ID DESC";
                using (SqlCommand commandER = new SqlCommand(strSQL, sqlCon))
                {
                    commandER.CommandType = CommandType.Text;
                    commandER.Parameters.AddWithValue("@PredictedObservedDetailsId", predictedObservedId);

                    SqlDataReader reader = commandER.ExecuteReader();
                    dtResults.Load(reader);
                    reader.Close();
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
