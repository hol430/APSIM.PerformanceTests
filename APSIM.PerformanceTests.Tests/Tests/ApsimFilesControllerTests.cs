using APSIM.PerformanceTests.Models;
using APSIM.PerformanceTests.Service;
using APSIM.PerformanceTests.Service.Controllers;
using APSIM.PerformanceTests.Service.Extensions;
using APSIM.Shared.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APSIM.PerformanceTests.Tests
{
    /// <summary>
    /// todo: this should really be merged with DBFunctionTests,
    /// and these functions which interact with the DB should be
    /// moved into DBFunctions.
    /// </summary>
    /// <remarks>
    /// For now, this requires sql server to be installed on the
    /// machine running the tests. Before tests are run, you will
    /// need to cd to:
    /// 
    /// C:\Program Files\Microsoft SQL Server\130\Tools\Binn
    /// 
    /// And run:
    /// 
    /// SqlLocalDB.exe create LocalDBApp1
    /// SqlLocalDB.exe start LocalDBApp1
    /// 
    /// After tests have finished, run:
    /// 
    /// SqlLocalDB.exe delete LocalDBApp1
    /// </remarks>
    [TestFixture]
    public class ApsimFilesControllerTests
    {
        private DbConnection[] emptyConnections;
        private DbConnection[] populousConnections;

        [SetUp]
        public void CreateConnections()
        {
            emptyConnections = new DbConnection[]
            {
                Utility.CreateSQLiteDB(),
                Utility.CreateSqlServerDB(),
            };

            populousConnections = new DbConnection[]
            {
                Utility.CreatePopulatedSQLiteDB(),
                Utility.CreatePopulatedSqlServerDB(),
            };
        }

        [TearDown]
        public void CloseConnections()
        {
            foreach (DbConnection connection in emptyConnections)
                Utility.CloseDB(connection);

            foreach (DbConnection connection in populousConnections)
                Utility.CloseDB(connection);
        }

        /// <summary>
        /// Test acceptance of certain variables being of the long (Int64) type.
        /// </summary>
        [Test]
        public void TestPostApsimFileWithLongSimulationID()
        {
            ApsimFile file = GetSimpleApsimFile();

            DataTable poData = new DataTable("PredictedObserved");
            poData.Columns.Add("SimulationID", typeof(long));
            poData.Columns.Add("Predicted.GrainWt", typeof(double));
            poData.Columns.Add("Observed.GrainWt", typeof(double));
            poData.Columns.Add("xval", typeof(double));
            poData.Rows.Add(1, 0.9, 1.1, 0.1);
            poData.Rows.Add(2, 0.5, 1.0, 0.1);
            file.PredictedObserved.ElementAt(0).Data = poData;

            foreach (DbConnection connection in populousConnections)
            {
                ApsimFilesController.InsertApsimFile(connection, file, out _, out _);
                using (DbCommand command = connection.CreateCommand(Utilities.Limit(connection, "SELECT SimulationsID FROM PredictedObservedValues", 1)))
                    Assert.AreEqual(1, command.ExecuteScalar());
            }
        }

        /// <summary>
        /// This test adds a simple apsimfile to an empty DB.
        /// </summary>
        [Test]
        public void TestPostSimpleApsimFile()
        {
            foreach (DbConnection connection in emptyConnections)
            {
                // Create a simple apsim file.
                ApsimFile file = GetSimpleApsimFile();
                PredictedObservedDetails poDetails = file.PredictedObserved.ElementAt(0);

                // Insert it into the database.
                ApsimFilesController.InsertApsimFile(connection, file, out string err, out int id);

                // Verify results.
                DataTable result = new DataTable();

                // Check ApsimFiles table.
                using (DbCommand command = connection.CreateCommand("SELECT * FROM ApsimFiles"))
                    using (DbDataReader reader = command.ExecuteReader())
                        result.Load(reader);

                Assert.NotNull(result);
                Assert.AreEqual(1, result.Rows.Count);
                Assert.AreEqual(10, result.Columns.Count);
                DataRow row = result.Rows[0];

                Assert.AreEqual(file.ID, row["ID"]);
                Assert.AreEqual(1, row["PullRequestID"]);
                Assert.AreEqual(file.FileName, row["FileName"]);
                Assert.AreEqual(file.FullFileName, row["FullFileName"]);
                if (connection is SQLiteConnection) // fixme - change sql server implementation to use varchar for dates????
                    Assert.AreEqual(file.RunDate.ToString("yyyy-MM-dd HH:mm:ss"), row["RunDate"]);
                else
                    Assert.AreEqual(file.RunDate, row["RunDate"]);
                Assert.AreEqual(file.IsMerged, row["IsMerged"]);
                Assert.AreEqual(file.StatsAccepted, row["StatsAccepted"]);
                Assert.AreEqual(file.SubmitDetails, row["SubmitDetails"]);
                Assert.AreEqual(file.AcceptedPullRequestId, row["AcceptedPullRequestId"]);
                //Assert.AreEqual("", row["AcceptedRunDate"]);//fixme

                // Check Simulations table.
                result = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM Simulations"))
                    using (DbDataReader reader = command.ExecuteReader())
                        result.Load(reader);

                Assert.AreEqual(2, result.Rows.Count);
                Assert.AreEqual(4, result.Columns.Count);

                row = result.Rows[0];
                Assert.AreEqual(1, row["ID"]);
                Assert.AreEqual(1, row["ApsimFilesID"]);
                Assert.AreEqual("sim1", row["Name"]);
                Assert.AreEqual(1, row["OriginalSimulationID"]);

                row = result.Rows[1];
                Assert.AreEqual(2, row["ID"]);
                Assert.AreEqual(1, row["ApsimFilesID"]);
                Assert.AreEqual("sim2", row["Name"]);
                Assert.AreEqual(2, row["OriginalSimulationID"]);

                // Check PredictedObservedDetails.
                result = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM PredictedObservedDetails"))
                    using (DbDataReader reader = command.ExecuteReader())
                        result.Load(reader);

                Assert.AreEqual(1, result.Rows.Count);
                Assert.AreEqual(11, result.Columns.Count);

                row = result.Rows[0];
                Assert.AreEqual(1, row["ID"]);
                Assert.AreEqual(file.ID, row["ApsimFilesID"]);
                Assert.AreEqual(poDetails.DatabaseTableName, row["TableName"]);
                Assert.AreEqual(poDetails.PredictedTableName, row["PredictedTableName"]);
                Assert.AreEqual(poDetails.ObservedTableName, row["ObservedTableName"]);
                Assert.AreEqual(poDetails.FieldNameUsedForMatch, row["FieldNameUsedForMatch"]);
                Assert.AreEqual(poDetails.FieldName2UsedForMatch, row["FieldName2UsedForMatch"]);
                Assert.AreEqual(poDetails.FieldName3UsedForMatch, row["FieldName3UsedForMatch"]);

                // todo: test the last 3 columns after implementing tests
                Assert.AreEqual(0, row["PassedTests"]);
                Assert.AreEqual(1, row["HasTests"]);
                Assert.AreEqual(DBNull.Value, row["AcceptedPredictedObservedDetailsID"]);

                // Check PredictedObservedValues.
                result = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM PredictedObservedValues"))
                    using (DbDataReader reader = command.ExecuteReader())
                        result.Load(reader);

                // #Rows = #Sims * #Variables * #PredictedObservedTables
                // Note that this isn't a general rule, because not every simulation
                // will necessarily generate predicted data for this p/o table.
                int nSims = file.Simulations.Rows.Count;
                int nVars = poDetails.Data.Columns.Cast<DataColumn>().Where(c => c.ColumnName.StartsWith("Predicted.")).Count();
                int nTables = file.PredictedObserved.Count();
                int nRows = nSims * nVars * nTables;

                Assert.AreEqual(nRows, result.Rows.Count);
                Assert.AreEqual(12, result.Columns.Count);

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    row = result.Rows[i];
                    Assert.AreEqual(i + 1, row["ID"]);
                    Assert.AreEqual(1, row["PredictedObservedDetailsID"]);

                    // Note: this will not always be true in the more general case.
                    Assert.AreEqual(poDetails.Data.Rows[i]["SimulationID"], row["SimulationsID"]);

                    Assert.AreEqual(poDetails.FieldNameUsedForMatch, row["MatchName"]);
                    Assert.AreEqual(poDetails.Data.Rows[i][poDetails.FieldNameUsedForMatch], row["MatchValue"]);

                    Assert.AreEqual(poDetails.FieldName2UsedForMatch, row["MatchName2"]);
                    if (!string.IsNullOrEmpty(poDetails.FieldName2UsedForMatch))
                        Assert.AreEqual(poDetails.Data.Rows[i][poDetails.FieldName2UsedForMatch], row["MatchValue2"]);
                    Assert.AreEqual(poDetails.FieldName3UsedForMatch, row["MatchName3"]);
                    if (!string.IsNullOrEmpty(poDetails.FieldName3UsedForMatch))
                        Assert.AreEqual(poDetails.Data.Rows[i][poDetails.FieldName3UsedForMatch], row["MatchValue3"]);

                    Assert.AreEqual("GrainWt", row["ValueName"]);
                    Assert.AreEqual(poDetails.Data.Rows[i]["Predicted.GrainWt"], row["PredictedValue"]);
                    Assert.AreEqual(poDetails.Data.Rows[i]["Observed.GrainWt"], row["ObservedValue"]);
                }

                // Check PredictedObservedTests.
                result = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM PredictedObservedTests"))
                    using (DbDataReader reader = command.ExecuteReader())
                        result.Load(reader);

                string[] variables = new[]
                {
                    "n",
                    "Slope",
                    "Intercept",
                    "SEslope",
                    "SEintercept",
                    "R2",
                    "RMSE",
                    "NSE",
                    "ME",
                    "MAE",
                    "RSR",
                };

                Assert.AreEqual(variables.Length, result.Rows.Count);
                Assert.AreEqual(12, result.Columns.Count);

                List<string> actualVariables = new List<string>();
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    row = result.Rows[i];

                    Assert.AreEqual(i + 1, row["ID"]);
                    Assert.AreEqual(1, row["PredictedObservedDetailsID"]);
                    Assert.AreEqual("GrainWt", row["Variable"]);
                    actualVariables.Add(row["Test"].ToString());

                    Assert.AreEqual(DBNull.Value, row["Accepted"]);
                }
                Assert.AreEqual(variables, actualVariables);

                Assert.AreEqual(2, GetCurrentValue(result, "n"), 1e-10);
                Assert.AreEqual(4, GetCurrentValue(result, "Slope"), 1e-10);
                Assert.AreEqual(-3.5, GetCurrentValue(result, "Intercept"), 1e-10);
                Assert.AreEqual(0, GetCurrentValue(result, "SEslope"), 1e-10);
                Assert.AreEqual(0, GetCurrentValue(result, "SEintercept"), 1e-10);
                Assert.AreEqual(1, GetCurrentValue(result, "R2"), 1e-10);
                Assert.AreEqual(0.380789, GetCurrentValue(result, "RMSE"), 1e-10);
                Assert.AreEqual(-57, GetCurrentValue(result, "NSE"), 1e-10);
                Assert.AreEqual(-0.35, GetCurrentValue(result, "ME"), 1e-10);
                Assert.AreEqual(0.35, GetCurrentValue(result, "MAE"), 1e-10);
                Assert.AreEqual(5.385165, GetCurrentValue(result, "RSR"), 1e-10);
            }
        }

        /// <summary>
        /// Ensure that when inserting a pull request, its accepted PR ID
        /// is correct.
        /// </summary>
        [Test]
        public void TestAcceptedStatsIDAfterInsert()
        {
            foreach (DbConnection connection in populousConnections)
            {
                // Let's pretend that we've accepted the existing pull request's stats.
                using (DbCommand command = connection.CreateCommand("UPDATE ApsimFiles SET StatsAccepted = 1"))
                    command.ExecuteNonQuery();

                // Now insert another pull request - its accepted stats ID should
                // be the ID of this pull request (1).
                ApsimFile file = GetSimpleApsimFile();
                file.PullRequestId = 2;
                ApsimFilesController.InsertApsimFile(connection, file, out _, out _);

                string sql = "SELECT AcceptedPullRequestId FROM ApsimFiles ORDER BY ID DESC";
                sql = Utilities.Limit(connection, sql, 1);
                using (DbCommand command = connection.CreateCommand(sql))
                    Assert.AreEqual(1, command.ExecuteScalar());
            }
        }

        /// <summary>
        /// When data for a pull request is added, we delete any existing data for the pull request ID.
        /// </summary>
        [Test]
        public void EnsureOldPullRequestDataIsDeleted()
        {
            foreach (DbConnection connection in populousConnections)
            {
                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM ApsimFiles"))
                    Assert.AreEqual(1, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM PredictedObservedDetails"))
                    Assert.AreEqual(1, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM PredictedObservedTests"))
                    Assert.AreEqual(11, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM PredictedObservedValues"))
                    Assert.AreEqual(2, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM Simulations"))
                    Assert.AreEqual(2, command.ExecuteScalar());

                // Now insert the new apsimfile. Number of rows in all tables should be unchanged.
                // The only difference is the run date.
                ApsimFile file = GetSimpleApsimFile();
                file.RunDate = file.RunDate.AddMinutes(1);
                ApsimFilesController.InsertApsimFile(connection, file, out _, out _);

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM ApsimFiles"))
                    Assert.AreEqual(1, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM PredictedObservedDetails"))
                    Assert.AreEqual(1, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM PredictedObservedTests"))
                    Assert.AreEqual(11, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM PredictedObservedValues"))
                    Assert.AreEqual(2, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM Simulations"))
                    Assert.AreEqual(2, command.ExecuteScalar());

                using (DbCommand command = connection.CreateCommand(Utilities.Limit(connection, "SELECT RunDate FROM ApsimFiles", 1)))
                {
                    object actual = command.ExecuteScalar();
                    if (connection is SQLiteConnection)
                        Assert.AreEqual("2020-01-01 00:01:00", actual);
                    else
                        Assert.AreEqual(new DateTime(2020, 1, 1).AddMinutes(1), actual);
                }
            }
        }

        private ApsimFile GetSimpleApsimFile()
        {
            DataTable simsTable = TableFactory.CreateEmptyApsimSimulationsTable();
            simsTable.Rows.Add(1, "sim1", null);
            simsTable.Rows.Add(2, "sim2", null);

            DataTable poData = new DataTable("PredictedObserved");
            poData.Columns.Add("SimulationID", typeof(int));
            poData.Columns.Add("Predicted.GrainWt", typeof(double));
            poData.Columns.Add("Observed.GrainWt", typeof(double));
            poData.Columns.Add("xval", typeof(double));
            poData.Rows.Add(1, 0.9, 1.1, 0.1);
            poData.Rows.Add(2, 0.5, 1.0, 0.1);

            PredictedObservedDetails poDetails = new PredictedObservedDetails()
            {
                DatabaseTableName = "PredictedObserved",
                PredictedTableName = "Report",
                ObservedTableName = "HarvestReport",
                FieldNameUsedForMatch = "xval",
                FieldName2UsedForMatch = string.Empty,
                FieldName3UsedForMatch = string.Empty,
                Data = poData,
            };

            return new ApsimFile()
            {
                ID = 1,
                AcceptedPullRequestId = -1,
                FileName = "wheat.apsimx",
                FullFileName = "~/wheat.apsimx",
                IsMerged = true,
                PullRequestId = 1,
                RunDate = new DateTime(2020, 1, 1),
                StatsAccepted = true,
                SubmitDetails = "submitdetails",
                Simulations = simsTable,
                PredictedObserved = new List<PredictedObservedDetails>() { poDetails },
            };
        }

        /// <summary>
        /// Gets the current value of a given test.
        /// E.g. the value of rmse for a given stat in a given pull request.
        /// The stat/pull request selection are encpsulated by the table parameter.
        /// </summary>
        /// <param name="table">All rows from the PredictedObservedTests table for a given pull request.</param>
        /// <param name="testName">Name of the test - e.g. rmse, nse, etc.</param>
        private double GetCurrentValue(DataTable table, string testName)
        {
            return (double)table.AsEnumerable().First(r => (string)r["Test"] == testName)["Current"];
        }

        /// <summary>
        /// Tests the <see cref="ApsimFilesController.GetApsimFile(int)"/> function.
        /// </summary>
        [Test]
        public void TestGetApsimFile()
        {
            foreach (DbConnection connection in emptyConnections)
            {
                // There are no apsim files.
                Assert.AreEqual(0, ApsimFilesController.GetApsimFiles(connection, 0).Count);
                Assert.AreEqual(0, ApsimFilesController.GetApsimFiles(connection, 1).Count);
            }

            foreach (DbConnection connection in populousConnections)
            {
                // There should be 1 apsim file, with ID 1.
                Assert.AreEqual(0, ApsimFilesController.GetApsimFiles(connection, 0).Count);

                List<ApsimFile> files = ApsimFilesController.GetApsimFiles(connection, 1);
                Assert.AreEqual(1, files.Count);
                Assert.AreEqual(1, files[0].ID);
            }
        }

        /// <summary>
        /// Tests the <see cref="ApsimFilesController.GetAllApsimFiles"/> function.
        /// </summary>
        [Test]
        public void TestGetAllApsimFiles()
        {
            foreach (DbConnection connection in emptyConnections)
                Assert.AreEqual(0, ApsimFilesController.GetAllApsimFiles(connection).Count);

            foreach (DbConnection connection in populousConnections)
            {
                List<ApsimFile> files = ApsimFilesController.GetAllApsimFiles(connection);

                Assert.AreEqual(1, files.Count);
                Assert.AreEqual(1, files[0].ID);
            }
        }

        /// <summary>
        /// Tests the <see cref="ApsimFilesController.DeleteByPullRequestId(int)"/> function.
        /// </summary>
        [Test]
        public void TestDeleteByPullRequetID()
        {
            foreach (DbConnection connection in emptyConnections)
            {
                ApsimFilesController.DeleteByPullRequest(connection, 0);
                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM ApsimFiles"))
                    Assert.AreEqual(0, command.ExecuteScalar());
            }

            foreach (DbConnection connection in populousConnections)
            {
                // This database contains data for a single pull request, with ID 1.

                // Deleting pull request with ID 0 should not remove any rows.
                ApsimFilesController.DeleteByPullRequest(connection, 0);
                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM ApsimFiles"))
                    Assert.AreEqual(1, command.ExecuteScalar());

                // Deleting pull request with ID 1 should remove the only row.
                ApsimFilesController.DeleteByPullRequest(connection, 1);
                using (DbCommand command = connection.CreateCommand("SELECT COUNT(*) FROM ApsimFiles"))
                    Assert.AreEqual(0, command.ExecuteScalar());
            }
        }
    }
}
