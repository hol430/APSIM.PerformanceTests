using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace APSIM.PerformanceTests.Tests
{
    public static class TableFactory
    {
        /// <summary>
        /// Returns an empty template datatable which mimics the Apsim
        /// _Simulations table structure.
        /// </summary>
        public static DataTable CreateEmptyApsimSimulationsTable()
        {
            DataTable table = new DataTable("_Simulations");
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("FolderName", typeof(string));

            return table;
        }

        /// <summary>
        /// Returns an empty template datatable which mimics the
        /// Simulations table from the performance tests DB schema.
        /// </summary>
        public static DataTable CreateEmptySimulationsTable()
        {
            DataTable table = new DataTable("Simulations");
            table.Columns.Add("ApsimFilesID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("OriginalSimulationID", typeof(int));

            return table;
        }

        /// <summary>
        /// Returns an empty template datatable which mimics the
        /// ApsimFiles table from the performance tests DB schema.
        /// </summary>
        public static DataTable CreateEmptyApsimFilesTable()
        {
            DataTable table = new DataTable("ApsimFiles");
            table.Columns.Add("PullRequestId", typeof(int));
            table.Columns.Add("FileName", typeof(string));
            table.Columns.Add("FullFileName", typeof(string));
            table.Columns.Add("RunDate", typeof(DateTime));
            table.Columns.Add("StatsAccepted", typeof(int));
            table.Columns.Add("IsMerged", typeof(int));
            table.Columns.Add("SubmitDetails", typeof(string));
            table.Columns.Add("AcceptedPullRequestId", typeof(int));
            table.Columns.Add("AcceptedRunDate", typeof(DateTime));

            return table;
        }

        public static DataTable CreateEmptyPredictedObservedDetailsTable()
        {
            DataTable table = new DataTable("PredictedObservedDetails");
            table.Columns.Add("ApsimFilesID", typeof(int));
            table.Columns.Add("TableName", typeof(string));
            table.Columns.Add("PredictedTableName", typeof(string));
            table.Columns.Add("ObservedTableName", typeof(string));
            table.Columns.Add("FieldNameUsedForMatch", typeof(string));
            table.Columns.Add("FieldName2UsedForMatch", typeof(string));
            table.Columns.Add("FieldName3UsedForMatch", typeof(string));
            table.Columns.Add("PassedTests", typeof(double));
            table.Columns.Add("HasTests", typeof(int)); // Should this be bool?
            table.Columns.Add("AcceptedPredictedObservedDetailsID", typeof(int));

            return table;
        }

        /// <summary>
        /// Returns an empty template datatable which mimics the
        /// PredictedObservedTests table from the performance tests DB schema.
        /// </summary>
        public static DataTable CreateEmptyPredictedObservedTestsTable()
        {
            DataTable table = new DataTable("PredictedObservedTests");
            table.Columns.Add("PredictedObservedDetailsID", typeof(int));
            table.Columns.Add("Variable", typeof(string));
            table.Columns.Add("Test", typeof(string));
            table.Columns.Add("Accepted", typeof(double));
            table.Columns.Add("Current", typeof(double));
            table.Columns.Add("Difference", typeof(double));
            table.Columns.Add("PassedTest", typeof(int));
            table.Columns.Add("AcceptedPredictedObservedTestsID", typeof(int));
            table.Columns.Add("IsImprovement", typeof(int));
            table.Columns.Add("SortOrder", typeof(int));
            table.Columns.Add("DifferencePercent", typeof(double));

            return table;
        }

        /// <summary>
        /// Returns an empty template datatable which mimics the
        /// PredictedObservedValues table from the performance tests
        /// DB schema.
        /// </summary>
        public static DataTable CreateEmptyPredictedObservedValuesTable()
        {
            DataTable table = new DataTable("PredictedObservedValues");
            table.Columns.Add("PredictedObservedDetailsID", typeof(int));
            table.Columns.Add("SimulationsID", typeof(int));
            table.Columns.Add("MatchName", typeof(string));
            table.Columns.Add("MatchValue", typeof(object));
            table.Columns.Add("MatchName2", typeof(string));
            table.Columns.Add("MatchValue2", typeof(object));
            table.Columns.Add("MatchName3", typeof(string));
            table.Columns.Add("MatchValue3", typeof(object));
            table.Columns.Add("ValueName", typeof(string));
            table.Columns.Add("PredictedValue", typeof(double));
            table.Columns.Add("ObservedValue", typeof(double));

            return table;
        }

        /// <summary>
        /// Returns an empty template datatable which mimics the
        /// AcceptStatsLogs table from the performance tests DB schema.
        /// </summary>
        public static DataTable CreateEmptyAcceptStatsLogsTable()
        {
            DataTable table = new DataTable("AcceptStatsLogs");
            table.Columns.Add("PullRequestId", typeof(int));
            table.Columns.Add("SubmitPerson", typeof(string));
            table.Columns.Add("SubmitDate", typeof(DateTime));
            table.Columns.Add("LogPerson", typeof(string));
            table.Columns.Add("LogReason", typeof(string));
            table.Columns.Add("LogStatus", typeof(int));
            table.Columns.Add("LogAcceptDate", typeof(DateTime));
            table.Columns.Add("StatsPullRequestId", typeof(int));
            table.Columns.Add("FileCount", typeof(int));

            return table;
        }
    }
}
