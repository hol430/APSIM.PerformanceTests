using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using APSIM.PerformanceTests.Portal.Models;
using System.Data.SqlClient;


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
    public static List<vSimulationPredictedObserved> GetByPredictedObservedIdAndVariable(int predictedObservedId, string variable)
    {
        //TODO:  Convert this to Indexed View in Sql Server
        using (ApsimDBContext context = new ApsimDBContext())
        {
            SqlParameter param1 = new SqlParameter("@PredictedObservedId", predictedObservedId);
            SqlParameter param2 = new SqlParameter("@ValueName", variable);

            var results = context.Database
                                 .SqlQuery<vSimulationPredictedObserved>("usp_GetByPredictedObservedIdAndVariable @PredictedObservedId, @ValueName", 
                                                                         param1, param2).ToList();

            //var results = (from pov in context.PredictedObservedValues
            //               join pod in context.PredictedObservedDetails on pov.PredictedObservedDetailsID equals pod.ID
            //               join s in context.Simulations on pov.SimulationsID equals s.ID
            //               where pov.PredictedObservedDetailsID == predictedObservedId && pov.ValueName == variable
            //               select new vSimulationPredictedObserved()
            //               {
            //                   TableName = pod.TableName,
            //                   SimulationName = s.Name,
            //                   MatchNames = (pov.MatchName + " " + pov.MatchName2 + " " + pov.MatchName3).Trim(),
            //                   MatchValues = (pov.MatchValue + " " + pov.MatchValue2 + " " + pov.MatchValue3).Trim(),
            //                   ValueName = pov.ValueName,
            //                   PredictedValue = ((pov.PredictedValue.HasValue) ? (double?)Math.Round((double)pov.PredictedValue, 3) : 0),
            //                   ObservedValue = ((pov.ObservedValue.HasValue) ? (double?)Math.Round((double)pov.ObservedValue, 3) : 0),
            //                   Difference = Math.Round(((pov.PredictedValue.HasValue) ? (double)pov.PredictedValue : 0) - ((pov.ObservedValue.HasValue) ? (double)pov.ObservedValue : 0), 3)
            //               })
            //               .Distinct()
            //               .ToList();

            return results;
        }
    }

    /// <summary>
    /// This takes two PredictedObservedDetails ID's, and returns a comparison of the values for both sets of PredictedObservedValues. 
    /// This is used to compare the 'Current' and 'Accepted' Values for a specific variable.
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="currentPoID"></param>
    /// <param name="acceptedPoId"></param>
    /// <returns></returns>
    public static List<vCurrentAndAccepted> GetCurrentAcceptedValues(string variable, int currentPoID, int acceptedPoId)
    {
        //var results = new List<vCurrentAndAccepted>();
        //List <vSimulationPredictedObserved> PODetails = GetByPredictedObservedIdAndVariable(currentPoID, variable);
        //List<vSimulationPredictedObserved> PODetails2 = GetByPredictedObservedIdAndVariable(acceptedPoId, variable);
        //    results = (from PO1 in PODetails
        //            join PO2 in PODetails2
        //            on new { PO1.TableName, PO1.SimulationName, PO1.ValueName, PO1.MatchNames, PO1.MatchValues }
        //            equals new { PO2.TableName, PO2.SimulationName, PO2.ValueName, PO2.MatchNames, PO2.MatchValues }
        //            select new vCurrentAndAccepted
        //            {
        //                TableName = PO1.TableName,
        //                SimulationName = PO1.SimulationName,
        //                MatchNames = PO1.MatchNames,
        //                MatchValues = PO1.MatchValues,
        //                ValueName = PO1.ValueName,
        //                CurrentPredictedValue = (double?)Math.Round((double)PO1.PredictedValue, 3),
        //                CurrentObservedValue = ((PO1.ObservedValue.HasValue) ? (double?)Math.Round((double)PO1.ObservedValue, 3) : 0),
        //                CurrentDifference = ((PO1.Difference.HasValue) ? (double?)Math.Round((double)PO1.Difference, 3) : 0),
        //                AcceptedPredictedValue = (double?)Math.Round((double)PO2.PredictedValue, 3),
        //                AcceptedObservedValue = ((PO2.ObservedValue.HasValue) ? (double?)Math.Round((double)PO2.ObservedValue, 3) : 0),
        //                AcceptedDifference = ((PO2.Difference.HasValue) ? (double?)Math.Round((double)PO2.Difference, 3) : 0),
        //                DifferenceDifference = ((PO1.Difference.HasValue && PO2.Difference.HasValue) ? (double?)Math.Round((double)PO1.Difference - (double)PO2.Difference, 3) : 0)
        //            })
        //            .Distinct()
        //            .ToList();

        using (ApsimDBContext context = new ApsimDBContext())
        {
            SqlParameter param1 = new SqlParameter("@CurrentPredictedObservedId", currentPoID);
            SqlParameter param2 = new SqlParameter("@AcceptedPredictedObservedId", acceptedPoId);
            SqlParameter param3 = new SqlParameter("@ValueName", variable);

            var results = context.Database
                            .SqlQuery<vCurrentAndAccepted>("usp_GetCurrentAcceptedValues @CurrentPredictedObservedId, @AcceptedPredictedObservedId, @ValueName", 
                                                            param1, param2, param3).ToList();
            return results;
        }
    }

    /// <summary>
    /// This is used tor retrieve the Tests details for a specifice Predicted Observed Id and variable.
    /// </summary>
    /// <param name="currentPoID"></param>
    /// <returns></returns>
    public static List<PredictedObservedTest> GetCurrentAcceptedTests(int currentPoID)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            //modLMC - 24/08/2017 - only retrieve reduced set of stats data (n, R2, RMSE, NSE and RSR) as per email from Dean.
            return context.PredictedObservedTests
                .Where(v => v.PredictedObservedDetailsID == currentPoID )
                .Distinct()
                .OrderBy(v => v.ID)
                .ToList();
        }
    }

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
            return context.PredictedObservedTests
                .Where(v => testList.Contains(v.Test) && v.PredictedObservedDetailsID == currentPoID)
                .Distinct()
                .OrderBy(v => v.ID)
                .ToList();
        }
    }

    /// <summary>
    /// This is used tor retrieve the Tests details for a specifice Predicted Observed Id and variable.
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="currentPoID"></param>
    /// <returns></returns>
    public static List<PredictedObservedTest> GetCurrentAcceptedTests(string variable, int currentPoID)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return (from pot in context.PredictedObservedTests
                    where pot.PredictedObservedDetailsID == currentPoID && pot.Variable == variable
                    select pot)
                .OrderBy(v => v.Variable)
                .ThenBy(v => v.Test)
                .ToList();
        }
    }

}
