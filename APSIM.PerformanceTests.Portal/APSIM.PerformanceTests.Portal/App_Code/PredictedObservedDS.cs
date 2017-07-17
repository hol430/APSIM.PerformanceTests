using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using APSIM.PerformanceTests.Portal.Models;


public class PredictedObservedDS
{

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

    public static PredictedObservedDetail GetByPredictedObservedID(int predictedObservedId)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return context.PredictedObservedDetails
                .Where(d => d.ID == predictedObservedId)
                .SingleOrDefault();
        }
    }

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


    public static List<vSimulationPredictedObserved> GetByPredictedObservedIdAndVariable(int predictedObservedId, string variable)
    {
        using (ApsimDBContext context = new ApsimDBContext())
        {
            return (from pov in context.PredictedObservedValues
                    join pod in context.PredictedObservedDetails on pov.PredictedObservedDetailsID equals pod.ID
                    join s in context.Simulations on pov.SimulationsID equals s.ID
                    where pov.PredictedObservedDetailsID == predictedObservedId && pov.ValueName == variable
                    select new vSimulationPredictedObserved()
                    {
                        TableName = pod.TableName,
                        SimulationName = s.Name,
                        MatchNames = (pov.MatchName + " " + pov.MatchName2 + " " + pov.MatchName3).Trim(),
                        MatchValues = (pov.MatchValue + " " + pov.MatchValue2 + " " + pov.MatchValue3).Trim(),
                        ValueName = pov.ValueName,
                        PredictedValue = ((pov.PredictedValue.HasValue) ? (double?)Math.Round((double)pov.PredictedValue, 3) : 0),
                        ObservedValue = ((pov.ObservedValue.HasValue) ? (double?)Math.Round((double)pov.ObservedValue, 3) : 0),
                        Difference = Math.Round(((pov.PredictedValue.HasValue) ? (double)pov.PredictedValue : 0) - ((pov.ObservedValue.HasValue) ? (double)pov.ObservedValue : 0), 3),
                    })
                    .Distinct()
                    .ToList();
        }
    }

    public static List<vCurrentAndAccepted> GetCurrentAcceptedValues(string variable, int currentPoID, int acceptedPoId)
    {
        List<vSimulationPredictedObserved> PODetails = GetByPredictedObservedIdAndVariable(currentPoID, variable);
        List<vSimulationPredictedObserved> PODetails2 = GetByPredictedObservedIdAndVariable(acceptedPoId, variable);

        return (from PO1 in PODetails
                join PO2 in PODetails2
                on new { PO1.TableName, PO1.SimulationName, PO1.ValueName, PO1.MatchNames, PO1.MatchValues }
                equals new { PO2.TableName, PO2.SimulationName, PO2.ValueName, PO2.MatchNames, PO2.MatchValues }
                select new vCurrentAndAccepted
                {
                    TableName = PO1.TableName,
                    SimulationName = PO1.SimulationName,
                    MatchNames = PO1.MatchNames,
                    MatchValues = PO1.MatchValues,
                    ValueName = PO1.ValueName,
                    CurrentPredictedValue = (double?)Math.Round((double)PO1.PredictedValue, 3),
                    CurrentObservedValue = ((PO1.ObservedValue.HasValue) ? (double?)Math.Round((double)PO1.ObservedValue, 3) : 0),
                    CurrentDifference = ((PO1.Difference.HasValue) ? (double?)Math.Round((double)PO1.Difference, 3) : 0),
                    AcceptedPredictedValue = (double?)Math.Round((double)PO2.PredictedValue, 3),
                    AcceptedObservedValue = ((PO2.ObservedValue.HasValue) ? (double?)Math.Round((double)PO2.ObservedValue, 3) : 0),
                    AcceptedDifference = ((PO2.Difference.HasValue) ? (double?)Math.Round((double)PO2.Difference, 3) : 0),
                    DifferenceDifference = ((PO1.Difference.HasValue && PO2.Difference.HasValue) ? (double?)Math.Round((double)PO1.Difference - (double)PO2.Difference, 3) : 0)
                })
                .Distinct()
                .ToList();
    }
}
