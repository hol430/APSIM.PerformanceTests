using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APSIM.PerformanceTests.Service;
using APSIM.Shared.Utilities;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;
using APSIM.PerformanceTests.Service.Extensions;
using APSIM.PerformanceTests.Models;

namespace APSIM.PerformanceTests.Tests
{
    [TestFixture]
    public class DBFunctionTests
    {
        /// <summary>
        /// Tests the <see cref="DBFunctions.GetFileCount(DbConnection, int)"/> function.
        /// </summary>
        [Test]
        public void TestGetFileCount()
        {
            using (SQLiteConnection connection = Utility.CreateSQLiteConnection())
                // todo: why is this 0?
                Assert.AreEqual(0, DBFunctions.GetFileCount(connection, 1));

            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                Assert.AreEqual(0, DBFunctions.GetFileCount(connection, 0));
                Assert.AreEqual(1, DBFunctions.GetFileCount(connection, 1));

                // Add 3 more apsim files.
                DataTable apsimFiles = TableFactory.CreateEmptyApsimFilesTable();
                apsimFiles.Rows.Add(1, "wheat.apsimx", "~/wheat.apsimx", new DateTime(2020, 1, 1), 0, 1, "submitdetails", -1, null);
                apsimFiles.Rows.Add(1, "wheat.apsimx", "~/wheat.apsimx", new DateTime(2020, 1, 1), 0, 1, "submitdetails", -1, null);
                apsimFiles.Rows.Add(1, "wheat.apsimx", "~/wheat.apsimx", new DateTime(2020, 1, 1), 0, 1, "submitdetails", -1, null);
                Utility.InsertDataIntoDatabase(connection, apsimFiles);

                // At this point, we haven't added any P/O details for these files,
                // so the file count should still be 1.
                Assert.AreEqual(1, DBFunctions.GetFileCount(connection, 1));

                DataTable poDetails = TableFactory.CreateEmptyPredictedObservedDetailsTable();
                poDetails.Rows.Add(1, "PredictedObserved", "HarvestReport", "Observations", "xval", null, null, 0, 1, null);
                poDetails.Rows.Add(1, "PredictedObserved", "HarvestReport", "Observations", "xval", null, null, 0, 1, null);
                poDetails.Rows.Add(1, "PredictedObserved", "HarvestReport", "Observations", "xval", null, null, 0, 1, null);
                Utility.InsertDataIntoDatabase(connection, poDetails);

                Assert.AreEqual(0, DBFunctions.GetFileCount(connection, 0));
                Assert.AreEqual(4, DBFunctions.GetFileCount(connection, 1));
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.GetAcceptedFileCount(DbConnection)"/> function.
        /// </summary>
        [Test]
        public void TestGetAcceptedFileCount()
        {
            using (SQLiteConnection connection = Utility.CreateSQLiteConnection())
                Assert.Throws<Exception>(() => DBFunctions.GetAcceptedFileCount(connection));

            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                Assert.Throws<Exception>(() => DBFunctions.GetAcceptedFileCount(connection));

                DataTable acceptStatsLogs = TableFactory.CreateEmptyAcceptStatsLogsTable();
                acceptStatsLogs.Rows.Add(1, "foo", new DateTime(2020, 1, 2), "bar", "why not", 1, new DateTime(2020, 1, 2), 1, 3);
                acceptStatsLogs.Rows.Add(1, "foo", new DateTime(2020, 1, 2), "baz", "reasons", 1, new DateTime(2020, 1, 2), 0, 4);
                Utility.InsertDataIntoDatabase(connection, acceptStatsLogs);

                Assert.AreEqual(4, DBFunctions.GetAcceptedFileCount(connection));
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.GetPercentPassed(DbConnection, int)"/> function.
        /// </summary>
        [Test]
        public void TestGetPercentPassed()
        {
            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                // For now, throw if pull request not found.
                Assert.Throws<Exception>(() => DBFunctions.GetPercentPassed(connection, 0));

                // 0 out of 1 tables passed the tests.
                Assert.AreEqual(0, DBFunctions.GetPercentPassed(connection, 1));

                // Add 2 more P/O tables, both of which passed the tests.
                DataTable poDetails = TableFactory.CreateEmptyPredictedObservedDetailsTable();
                poDetails.Rows.Add(1, "DailyPredictedObserved", "Report", "DailyObs", "xval2", null, null, 100, 1, null);
                poDetails.Rows.Add(1, "DailyPredictedObserved", "Report", "DailyObs", "xval2", null, null, 100, 1, null);
                Utility.InsertDataIntoDatabase(connection, poDetails);

                // 2 out of 3 tables passed the tests.
                Assert.AreEqual(66, DBFunctions.GetPercentPassed(connection, 1));

                // Delete the failed table (if only it were this easy irl).
                using (DbCommand command = connection.CreateCommand("DELETE FROM PredictedObservedDetails WHERE PassedTests = 0;"))
                    command.ExecuteNonQuery();

                // 2 out of 2 tables passed the tests.
                Assert.AreEqual(100, DBFunctions.GetPercentPassed(connection, 1));
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.GetApsimFilesRelatedPredictedObservedData(DbConnection, int)"/> function.
        /// </summary>
        [Test]
        public void TestGetApsimFilesRelatedPredictedObservedDetailsData()
        {
            using (SQLiteConnection connection = Utility.CreateSQLiteConnection())
            {
                List<ApsimFile> apsimFiles = DBFunctions.GetApsimFilesRelatedPredictedObservedData(connection, 1);
                Assert.AreEqual(0, apsimFiles.Count);
            }

            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                Assert.AreEqual(0, DBFunctions.GetApsimFilesRelatedPredictedObservedData(connection, 0).Count);
                List<ApsimFile> apsimFiles = DBFunctions.GetApsimFilesRelatedPredictedObservedData(connection, 1);
                Assert.AreEqual(1, apsimFiles.Count);

                ApsimFile file = apsimFiles[0];
                Assert.AreEqual(-1, file.AcceptedPullRequestId);
                Assert.AreEqual("wheat.apsimx", file.FileName);
                Assert.AreEqual("~/wheat.apsimx", file.FullFileName);
                Assert.AreEqual(1, file.ID);
                Assert.AreEqual(true, file.IsMerged);
                Assert.AreEqual(1, file.PullRequestId);
                Assert.AreEqual(new DateTime(2020, 1, 1), file.RunDate);
                Assert.AreEqual(null, file.Simulations); // todo: should we implement this?
                Assert.AreEqual(false, file.StatsAccepted);
                Assert.AreEqual("submitdetails", file.SubmitDetails);

                Assert.AreEqual(1, file.PredictedObserved.Count());
                PredictedObservedDetails details = file.PredictedObserved.ElementAt(0);

                Assert.AreEqual(0, details.AcceptedPredictedObservedDetailsId);
                Assert.AreEqual(null, details.ApsimFile); // Not brave enough to change this yet
                Assert.AreEqual(1, details.ApsimID);
                Assert.AreEqual(null, details.Data); // Not brave enough to change this yet
                Assert.AreEqual("PredictedObserved", details.DatabaseTableName);
                Assert.AreEqual("xval", details.FieldNameUsedForMatch);
                Assert.AreEqual(null, details.FieldName2UsedForMatch);
                Assert.AreEqual(null, details.FieldName3UsedForMatch);
                Assert.AreEqual(1, details.HasTests);
                Assert.AreEqual(1, details.ID);
                Assert.AreEqual("Observations", details.ObservedTableName);
                Assert.AreEqual(0, details.PassedTests);
                Assert.AreEqual("HarvestReport", details.PredictedTableName);
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.GetAcceptedPredictedObservedDetailsId(DbConnection, int, string, PredictedObservedDetails)"/> function.
        /// </summary>
        [Test]
        public void TestGetPODetailsID()
        {
            using (SQLiteConnection connection = Utility.CreateSQLiteConnection())
            {
                PredictedObservedDetails details = new PredictedObservedDetails();
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 0, null, details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 0, "wheat.apsimx", details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 1, null, details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 1, "wheat.apsimx", details));

                details.DatabaseTableName = "PredictedObserved";

                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 0, null, details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 0, "wheat.apsimx", details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 1, null, details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 1, "wheat.apsimx", details));
            }

            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                PredictedObservedDetails details = new PredictedObservedDetails();
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 0, null, details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 0, "wheat.apsimx", details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 1, null, details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 1, "wheat.apsimx", details));

                details.DatabaseTableName = "PredictedObserved";

                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 0, null, details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 0, "wheat.apsimx", details));
                Assert.AreEqual(0, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 1, null, details));
                Assert.AreEqual(1, DBFunctions.GetAcceptedPredictedObservedDetailsId(connection, 1, "wheat.apsimx", details));
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.GetPredictedObservedTestsData(DbConnection, int)"/> function.
        /// </summary>
        [Test]
        public void TestGetPOTests()
        {
            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                DataTable result = DBFunctions.GetPredictedObservedTestsData(connection, 1);

                Assert.AreEqual(11, result.Rows.Count);

                DataRow row = result.Rows[0];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("Intercept", row["Test"]);
                Assert.AreEqual(-3.5, row["Accepted"]);
                Assert.AreEqual(3, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[1];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("MAE", row["Test"]);
                Assert.AreEqual(0.35, row["Accepted"]);
                Assert.AreEqual(10, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[2];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("ME", row["Test"]);
                Assert.AreEqual(-0.35, row["Accepted"]);
                Assert.AreEqual(9, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[3];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("NSE", row["Test"]);
                Assert.AreEqual(-57, row["Accepted"]);
                Assert.AreEqual(8, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[4];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("R2", row["Test"]);
                Assert.AreEqual(1, row["Accepted"]);
                Assert.AreEqual(6, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[5];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("RMSE", row["Test"]);
                Assert.AreEqual(0.380789, row["Accepted"]);
                Assert.AreEqual(7, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[6];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("RSR", row["Test"]);
                Assert.AreEqual(5.385165, row["Accepted"]);
                Assert.AreEqual(11, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[7];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("SEintercept", row["Test"]);
                Assert.AreEqual(0, row["Accepted"]);
                Assert.AreEqual(5, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[8];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("SEslope", row["Test"]);
                Assert.AreEqual(0, row["Accepted"]);
                Assert.AreEqual(4, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[9];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("Slope", row["Test"]);
                Assert.AreEqual(4, row["Accepted"]);
                Assert.AreEqual(2, row["AcceptedPredictedObservedTestsID"]);

                row = result.Rows[10];
                Assert.AreEqual("GrainWt", row["Variable"]);
                Assert.AreEqual("n", row["Test"]);
                Assert.AreEqual(2, row["Accepted"]);
                Assert.AreEqual(1, row["AcceptedPredictedObservedTestsID"]);
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.GetPredictedObservedValues(System.Data.SqlClient.SqlConnection, int)"/> function.
        /// </summary>
        [Test]
        public void TestGetPOValues()
        {
            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                DataTable result = DBFunctions.GetPredictedObservedValues(connection, 1);

                Assert.AreEqual(2, result.Rows.Count);

                Assert.AreEqual(0.9, result.Rows[0]["PredictedValue"]);
                Assert.AreEqual(1.1, result.Rows[0]["ObservedValue"]);

                Assert.AreEqual(0.5, result.Rows[1]["PredictedValue"]);
                Assert.AreEqual(1, result.Rows[1]["ObservedValue"]);
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.AddPredictedObservedTestsData(DbConnection, string, int, string, DataTable)"/> function.
        /// </summary>
        [Test]
        public void TestAddPOTestsData()
        {
            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                DataTable poTests = TableFactory.CreateEmptyPredictedObservedTestsTable();
                //PredictedObservedDetailsID, Variable, Test, Accepted, Current, Difference, PassedTest, AcceptedPredictedObservedTestsID, IsImprovement, SortOrder, DifferencePercent
                poTests.Rows.Add(1, "TestVar", "n", 1, 2, 3, 0, null, 0, 0, 100);
                DBFunctions.AddPredictedObservedTestsData(connection, null, 1, null, poTests);

                poTests = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM PredictedObservedTests"))
                    using (DbDataReader reader = command.ExecuteReader())
                        poTests.Load(reader);

                Assert.AreEqual(12, poTests.Rows.Count);
                DataRow row = poTests.Rows[11];
                Assert.AreEqual(1, row["PredictedObservedDetailsID"]);
                Assert.AreEqual("TestVar", row["Variable"]);
                Assert.AreEqual("n", row["Test"]);
                Assert.AreEqual(1, row["Accepted"]);
                Assert.AreEqual(2, row["Current"]);
                Assert.AreEqual(3, row["Difference"]);
                Assert.AreEqual(false, row["PassedTest"]);
                Assert.AreEqual(DBNull.Value, row["AcceptedPredictedObservedTestsID"]);
                Assert.AreEqual(false, row["IsImprovement"]);
                Assert.AreEqual(0, row["SortOrder"]);
                Assert.AreEqual(3, row["Difference"]);
                Assert.AreEqual(300, row["DifferencePercent"]);

                DataTable poDetails = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM PredictedObservedDetails"))
                    using (DbDataReader reader = command.ExecuteReader())
                        poDetails.Load(reader);

                Assert.AreEqual(1, poDetails.Rows.Count);
                row = poDetails.Rows[0];
                Assert.AreEqual(1, row["ID"]);
                Assert.AreEqual(0, row["PassedTests"]);
                Assert.AreEqual(1, row["HasTests"]);
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.UpdatePredictedObservedDetails(DbConnection, int, int)"/> function.
        /// </summary>
        [Test]
        public void TestUpdatePODetails()
        {
            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                DBFunctions.UpdatePredictedObservedDetails(connection, 2, 1);
                // There's only 1 record in the table.
                using (DbCommand command = connection.CreateCommand("SELECT AcceptedPredictedObservedDetailsID FROM PredictedObservedDetails"))
                    Assert.AreEqual(2, command.ExecuteScalar());

            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.UpdateApsimFileAcceptedDetails(DbConnection, int, int, DateTime)"/> function.
        /// </summary>
        [Test]
        public void TestUpdateApsimFileAcceptedDetails()
        {
            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                DateTime date = new DateTime(2020, 1, 2);
                DBFunctions.UpdateApsimFileAcceptedDetails(connection, 1, 2, date);
                DataTable apsimFiles = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM ApsimFiles"))
                    using (DbDataReader reader = command.ExecuteReader())
                        apsimFiles.Load(reader);

                Assert.AreEqual(1, apsimFiles.Rows.Count);
                DataRow row = apsimFiles.Rows[0];
                Assert.AreEqual(1, row["PullRequestId"]);
                Assert.AreEqual(2, row["AcceptedPullRequestId"]);
                Assert.AreEqual(date.ToString("yyyy-MM-dd HH:mm:ss"), row["AcceptedRunDate"]);
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.UpdateAsStatsAccepted(DbConnection, string, AcceptStatsLog)"/> function.
        /// </summary>
        [Test]
        public void TestAcceptStats()
        {
            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                AcceptStatsLog log = new AcceptStatsLog()
                {
                    FileCount = 1,
                    LogAcceptDate = new DateTime(2020, 2, 1),
                    LogPerson = "Chazza",
                    LogReason = "felt like it",
                    LogStatus = true,
                    PullRequestId = 1,
                    StatsPullRequestId = -1,
                    SubmitDate = new DateTime(2020, 1, 1),
                    SubmitPerson = "Bazza",
                };

                DBFunctions.UpdateAsStatsAccepted(connection, "Accept", log);

                // Need to check AcceptStatsLogs and ApsimFiles.
                DataTable acceptStatsLog = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM AcceptStatsLogs"))
                using (DbDataReader reader = command.ExecuteReader())
                    acceptStatsLog.Load(reader);

                Assert.AreEqual(1, acceptStatsLog.Rows.Count);
                Assert.AreEqual(10, acceptStatsLog.Columns.Count);
                DataRow row = acceptStatsLog.Rows[0];

                Assert.AreEqual(1, row["ID"]);
                Assert.AreEqual(log.PullRequestId, row["PullRequestId"]);
                Assert.AreEqual(log.SubmitPerson, row["SubmitPerson"]);
                Assert.AreEqual(log.SubmitDate, row["SubmitDate"]);
                Assert.AreEqual(log.LogPerson, row["LogPerson"]);
                Assert.AreEqual(log.LogReason, row["LogReason"]);
                Assert.AreEqual(log.LogStatus, row["LogStatus"]);
                Assert.AreEqual(log.LogAcceptDate, row["LogAcceptDate"]);
                Assert.AreEqual(log.StatsPullRequestId, row["StatsPullRequestId"]);
                Assert.AreEqual(log.FileCount, row["FileCount"]);

                DataTable apsimFiles = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM ApsimFiles"))
                using (DbDataReader reader = command.ExecuteReader())
                    apsimFiles.Load(reader);

                Assert.AreEqual(1, apsimFiles.Rows.Count);
                row = apsimFiles.Rows[0];

                // Accepting the stats currently doesn't update the AcceptedPullRequestID of
                // any existing apsim files.
                Assert.AreEqual(-1, row["AcceptedPullRequestId"]);
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.GetLatestRunDateForPullRequest(DbConnection, int)"/> function.
        /// </summary>
        [Test]
        public void TestGetLatestPullRequestRunDate()
        {
            using (DbConnection connection = Utility.CreateSQLiteConnection())
                Assert.Throws<Exception>(() => DBFunctions.GetLatestRunDateForPullRequest(connection, 1));

            using (DbConnection connection = Utility.CreatePopulatedDB())
            {
                Assert.Throws<Exception>(() => DBFunctions.GetLatestRunDateForPullRequest(connection, 0));
                Assert.AreEqual(new DateTime(2020, 1, 1), DBFunctions.GetLatestRunDateForPullRequest(connection, 1));
            }
        }

        /// <summary>
        /// Tests the <see cref="DBFunctions.RenamePOTable(DbConnection, string, string, string)"/> function.
        /// </summary>
        [Test]
        public void TestRenamePOTable()
        {
            using (SQLiteConnection connection = Utility.CreatePopulatedDB())
            {
                DBFunctions.RenamePOTable(connection, "wheat.apsimx", "PredictedObserved", "foo");

                DataTable poDetails = new DataTable();
                using (DbCommand command = connection.CreateCommand("SELECT * FROM PredictedObservedDetails"))
                    using (DbDataReader reader = command.ExecuteReader())
                        poDetails.Load(reader);

                Assert.AreEqual(1, poDetails.Rows.Count);
                Assert.AreEqual("foo", poDetails.Rows[0]["TableName"]);
            }
        }
    }
}
