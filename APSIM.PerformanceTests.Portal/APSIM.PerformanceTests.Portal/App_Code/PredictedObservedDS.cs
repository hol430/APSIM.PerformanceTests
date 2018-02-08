using APSIM.PerformanceTests.Portal.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;


public class PredictedObservedDS
{
    //NOTE:  Dont forget that these need to have the build property set to compile

    /// <summary>
    /// Retrieves the Table Name for a Predicted Observed table, based on the Id (int)
    /// </summary>
    /// <param name="predictedObservedId"></param>
    /// <returns></returns>
    public static string GetFilenameByPredictedObservedID(int predictedObservedId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return (from pod in context.PredictedObservedDetails
                    where (pod.ID == predictedObservedId)
                    select pod.TableName)
                    .SingleOrDefault();
        }
    }

    /// <summary>
    /// Gets the Predicted Observed Details based on the id (int)
    /// </summary>
    /// <param name="predictedObservedId"></param>
    /// <returns></returns>
    public static PredictedObservedDetail GetByPredictedObservedID(int predictedObservedId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.PredictedObservedDetails
                .Where(d => d.ID == predictedObservedId)
                .SingleOrDefault();
        }
    }

    /// <summary>
    /// Thes the Predicted Observed Details ID, for a specific Pull Request ID (int), by matching specific 
    /// PredictedObservedDetails information.  This is used when the 'Accepted' PredictedObservedDetailsID is
    /// not saved with the 'Current' PredictedObservedDetails record.
    /// </summary>
    /// <param name="pullRequestId"></param>
    /// <param name="filename"></param>
    /// <param name="tablename"></param>
    /// <param name="predictedTableName"></param>
    /// <param name="observedTableName"></param>
    /// <param name="fieldNameUsedForMatch"></param>
    /// <returns></returns>
    public static int GetIDByMatchingDetails(int pullRequestId, string filename, string tablename, string predictedTableName, string observedTableName, string fieldNameUsedForMatch)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return (from pod in context.PredictedObservedDetails
                    join af in context.ApsimFiles on pod.ApsimFilesID equals af.ID
                    where af.PullRequestId == pullRequestId
                       && af.FileName == filename
                       && pod.TableName == tablename
                       && pod.PredictedTableName == predictedTableName
                       && pod.ObservedTableName == observedTableName
                       && pod.FieldNameUsedForMatch == fieldNameUsedForMatch
                    select pod.ID)
                    .SingleOrDefault();
        }
    }

    /// <summary>
    /// Thes the Predicted Observed Details ID, for a specific Pull Request ID (int), by matching specific 
    /// PredictedObservedDetails information.  This is used when the 'Accepted' PredictedObservedDetailsID is
    /// not saved with the 'Current' PredictedObservedDetails record.
    /// </summary>
    /// <param name="pullRequestId"></param>
    /// <param name="filename"></param>
    /// <param name="tablename"></param>
    /// <param name="predictedTableName"></param>
    /// <param name="observedTableName"></param>
    /// <param name="fieldNameUsedForMatch"></param>
    /// <returns></returns>
    public static int GetIDByMatchingDetails(int pullRequestId, string filename, string tablename)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return (from pod in context.PredictedObservedDetails
                    join af in context.ApsimFiles on pod.ApsimFilesID equals af.ID
                    where af.PullRequestId == pullRequestId
                       && af.FileName == filename
                       && pod.TableName == tablename
                    select pod.ID)
                    .SingleOrDefault();
        }
    }


    /// <summary>
    /// Returns a list of the Variables being reported on, in the PredictedObservedValues table,
    /// for a specific PredictedObservedDetailsID (int)
    /// </summary>
    /// <param name="predictedObservedId"></param>
    /// <returns></returns>
    public static List<vVariable> GetVariablesByPredictedObservedID(int predictedObservedId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.PredictedObservedValues
                .Where(v => v.PredictedObservedDetailsID == predictedObservedId)
                .Select(v => new vVariable
                {
                    Name = v.ValueName,
                    Value = v.ValueName
                })
                .Distinct()
                .OrderBy(v => v.Name)
                .ToList();
        }
    }


    /// <summary>
    /// Returns the PredictedObservedValues and corresponding Simulation details for a specific PredictedObserved Table and variable
    /// </summary>
    /// <param name="predictedObservedId"></param>
    /// <param name="variable"></param>
    /// <returns></returns>
    //public static List<vSimulationPredictedObserved> GetByPredictedObservedIdAndVariable(int predictedObservedId, string variable)
    //{
    //    //TODO:  Convert this to Indexed View in Sql Server
    //    using (ApsimDBContext context = new ApsimDBContext())
    //    {
    //        SqlParameter param1 = new SqlParameter("@PredictedObservedId", predictedObservedId);
    //        SqlParameter param2 = new SqlParameter("@ValueName", variable);

    //        var results = context.Database
    //                             .SqlQuery<vSimulationPredictedObserved>("usp_GetByPredictedObservedIdAndVariable @PredictedObservedId, @ValueName", 
    //                                                                     param1, param2).ToList();
    //        return results;
    //    }
    //}

    /// <summary>
    /// This takes two PredictedObservedDetails ID's, and returns a comparison of the values for both sets of PredictedObservedValues. 
    /// This is used to compare the 'Current' and 'Accepted' Values for a specific variable.
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="currentPoID"></param>
    /// <param name="acceptedPoId"></param>
    /// <returns></returns>
    //public static List<vCurrentAndAccepted> GetCurrentAcceptedValues(string variable, int currentPoID, int acceptedPoId)
    //{
    //    using (ApsimDBContext context = new ApsimDBContext())
    //    {
    //        SqlParameter param1 = new SqlParameter("@CurrentPredictedObservedId", currentPoID);
    //        SqlParameter param2 = new SqlParameter("@AcceptedPredictedObservedId", acceptedPoId);
    //        SqlParameter param3 = new SqlParameter("@ValueName", variable);

    //        var results = context.Database
    //                        .SqlQuery<vCurrentAndAccepted>("usp_GetCurrentAcceptedValues @CurrentPredictedObservedId, @AcceptedPredictedObservedId, @ValueName", 
    //                                                        param1, param2, param3).ToList();
    //        return results;
    //    }
    //}

    public static List<vCurrentAndAccepted> GetCurrentAcceptedValuesWithNulls(string variable, int currentPoID, int acceptedPoId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            SqlParameter param1 = new SqlParameter("@CurrentPredictedObservedId", currentPoID);
            SqlParameter param2 = new SqlParameter("@AcceptedPredictedObservedId", acceptedPoId);
            SqlParameter param3 = new SqlParameter("@ValueName", variable);

            var results = context.Database
                            .SqlQuery<vCurrentAndAccepted>("usp_GetCurrentAcceptedValuesWithNulls @CurrentPredictedObservedId, @AcceptedPredictedObservedId, @ValueName",
                                                            param1, param2, param3)
                            .Select(v => new vCurrentAndAccepted
                            {
                                ID = v.ID,
                                TableName = v.TableName,
                                SimulationName = v.SimulationName,
                                MatchNames = v.MatchNames,
                                MatchValues = v.MatchNames,
                                CurrentPredictedValue = (double?)v.CurrentPredictedValue,
                                CurrentObservedValue = (double?)v.CurrentObservedValue,
                                AcceptedPredictedValue = (double?)v.AcceptedPredictedValue,
                                AcceptedObservedValue = (double?)v.AcceptedObservedValue
                            })
                            .ToList();
            return results;
        }
    }

    public static List<vSimulationPredictedObserved> GetCurrentAcceptedSimulationValues(int currentPoID, int acceptedPoId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            SqlParameter param1 = new SqlParameter("@CurrentPredictedObservedId", currentPoID);
            SqlParameter param2 = new SqlParameter("@AcceptedPredictedObservedId", acceptedPoId);

            var results = context.Database
                .SqlQuery<vSimulationPredictedObserved>("usp_GetCurrentAcceptedSimulationValues @CurrentPredictedObservedId, @AcceptedPredictedObservedId", param1, param2)
                .Select(v => new vSimulationPredictedObserved
                {
                    ID = v.ID,
                    MatchNames = v.MatchNames,
                    MatchValues = v.MatchNames,
                    ValueName = v.ValueName,
                    CurrentPredictedValue = (double?)v.CurrentPredictedValue,
                    CurrentObservedValue = (double?)v.CurrentObservedValue,
                    AcceptedPredictedValue = (double?)v.AcceptedPredictedValue,
                    AcceptedObservedValue = (double?)v.AcceptedObservedValue,
                    SimulationName = v.SimulationName,
                    SimulationsID = (int)v.ID
                })
                .ToList();
            return results;
        }
    }

    /// <summary>
    /// This is used tor retrieve the Tests details for a specifice Predicted Observed Id and variable.
    /// </summary>
    /// <param name="currentPoID"></param>
    /// <returns>List<PredictedObservedTest></returns>
    public static List<PredictedObservedTest> GetCurrentAcceptedTestsSubset(int currentPoID)
    {
        List<string> testList = new List<string>();
        testList.Add("n");
        testList.Add("R2");
        testList.Add("RMSE");
        testList.Add("NSE");
        testList.Add("RSR");

        using (ApsimDBContext context = new ApsimDBContext())
        {
            //modLMC - 24/08/2017 - only retrieve reduced set of stats data (n, R2, RMSE, NSE and RSR) as per email from Dean.
            //modLMC - 31/01/2018 - modified to exclude records where both Current and Accepted are null (fudge until 'Service' is udpated to not add them).
            return context.PredictedObservedTests
                .Where(v => testList.Contains(v.Test) && v.PredictedObservedDetailsID == currentPoID && !(v.Current == null && v.Accepted == null))
                .Distinct()
                .OrderBy(v => v.PassedTest)
                .ThenByDescending(v => v.DifferencePercent)
                .ThenBy(v => v.SortOrder)
                .ThenBy(v => v.Test)
                .ToList();
        }
    }

    public static List<vPredictedObservedTests> GetCurrentAcceptedTestsDiffsSubset(int pullRequestId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            SqlParameter param1 = new SqlParameter("@PullRequestId", pullRequestId);

            var results = context.Database
                            .SqlQuery<vPredictedObservedTests>("usp_GetPredictedObservedTestswithDifferences @PullRequestId", param1)
                            .ToList();
            return results;
        }
    }



    public static List<vPredictedObservedTests> GetCurrentAcceptedTests(int pullRequestId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            SqlParameter param1 = new SqlParameter("@PullRequestId", pullRequestId);
            //[Test] IN ('n', 'R2', 'RMSE', 'NSE', 'RSR')
            var results = context.Database
                            .SqlQuery<vPredictedObservedTests>("usp_GetPredictedObservedTests @PullRequestId", param1)
                            .ToList();
            return results;
        }
    }

    public static List<vPredictedObservedTests> GetCurrentAcceptedTestsFiltered(int pullRequestId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            SqlParameter param1 = new SqlParameter("@PullRequestId", pullRequestId);
            //[Test] IN ('RMSE', 'NSE', 'Bias', 'RSR')
            var results = context.Database
                            .SqlQuery<vPredictedObservedTests>("usp_GetPredictedObservedTestsFiltered @PullRequestId", param1)
                            .ToList();
            return results;
        }
    }


    /// <summary>
    /// This is used tor retrieve the Tests details for a specifice Predicted Observed Id and variable.
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="currentPoID"></param>
    /// <returns></returns>
    public static List<PredictedObservedTest> GetPredictedObservedTest(int currentPoID, string variable)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.PredictedObservedTests
                .Where(v => v.PredictedObservedDetailsID == currentPoID && v.Variable == variable)
                .OrderBy(v => v.SortOrder)
                .ThenBy(v => v.Test)
                .ToList();
        }
    }

    public static List<vPredictedObservedTestsFormatted> GetPredictedObservedTestFormatted(int currentPoID, string variable)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            SqlParameter param1 = new SqlParameter("@PredictedObservedId", currentPoID);
            SqlParameter param2 = new SqlParameter("@Variable", variable);

            var results = context.Database
                            .SqlQuery<vPredictedObservedTestsFormatted>("usp_GetPredictedObservedTestsFormatted @PredictedObservedId, @Variable", param1, param2)
                            .ToList();
            return results;
        }
    }

}
