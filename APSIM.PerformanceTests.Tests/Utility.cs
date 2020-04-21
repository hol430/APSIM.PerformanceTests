using APSIM.PerformanceTests.Service.Extensions;
using APSIM.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APSIM.PerformanceTests.Tests
{
    public static class Utility
    {
        public static SqlConnection CreateSqlServerDB()
        {
            var connection = new SqlConnection(@"Server = (localdb)\LocalDBApp1; Integrated Security = true;");
            connection.Open();

            string dbName = Guid.NewGuid().ToString().Replace("-", "");
            using (DbCommand command = connection.CreateCommand($"CREATE DATABASE [{dbName}];"))// GO; USE [{dbName}]; GO;"))
                command.ExecuteNonQuery();
            using (DbCommand command = connection.CreateCommand($"USE [{dbName}];"))
                command.ExecuteNonQuery();

            Initialise(connection);

            return connection;
        }

        /// <summary>
        /// Creates, opens and returns a connection to an
        /// in-memory SQLite database with the standard
        /// APSIM.PerformanceTests schema.
        /// </summary>
        /// <returns></returns>
        public static SQLiteConnection CreateSQLiteDB()
        {
            var connection = new SQLiteConnection("Data Source=:memory:");
            connection.Open();
            Initialise(connection);

            return connection;
        }

        public static SqlConnection CreatePopulatedSqlServerDB()
        {
            SqlConnection connection = CreateSqlServerDB();
            Populate(connection);
            return connection;
        }

        internal static void CloseDB(DbConnection connection)
        {
            if (connection is SqlConnection)
            {
                string dbName;
                using (DbCommand command = connection.CreateCommand("SELECT DB_NAME();"))
                    dbName = (string)command.ExecuteScalar();
                using (DbCommand command = connection.CreateCommand($"USE MASTER; DROP DATABASE [{dbName}];"))
                    command.ExecuteNonQuery();
            }
            connection.Close();
        }

        public static SQLiteConnection CreatePopulatedSQLiteDB()
        {
            SQLiteConnection connection = CreateSQLiteDB();
            Populate(connection);
            return connection;
        }

        /// <summary>
        /// Insert DataTable into a database. This assumes that
        /// the table already exists in the database.
        /// </summary>
        /// <param name="connection">Connection to the database.</param>
        /// <param name="table">Table to be inserted.</param>
        public static void InsertDataIntoDatabase(DbConnection connection, DataTable table)
        {
            string[] colNames = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            string[] paramNames = colNames.Select(n => "@" + n).ToArray();
            string[] escapedColNames = colNames.Select(c => $"[{c}]").ToArray();
            string sql = $"INSERT INTO {table.TableName} ({string.Join(", ", escapedColNames)})\n" +
                         $"VALUES ({string.Join(", ", paramNames)});";

            using (DbCommand command = connection.CreateCommand(sql))
            {
                foreach (string param in paramNames)
                    command.AddParamWithValue(param, null);

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        string paramName = "@" + col.ColumnName;
                        command.Parameters[paramName].Value = row[col.ColumnName];
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void Populate(DbConnection connection)
        {
            DataTable simulations = TableFactory.CreateEmptySimulationsTable();
            simulations.Rows.Add(1, "sim1", 1);
            simulations.Rows.Add(1, "sim2", 2);
            InsertDataIntoDatabase(connection, simulations);

            DataTable apsimFiles = TableFactory.CreateEmptyApsimFilesTable();
            apsimFiles.Rows.Add(1, "wheat.apsimx", "~/wheat.apsimx", new DateTime(2020, 1, 1), 0, 1, "submitdetails", -1, null);
            InsertDataIntoDatabase(connection, apsimFiles);

            DataTable poDetails = TableFactory.CreateEmptyPredictedObservedDetailsTable();
            poDetails.Rows.Add(1, "PredictedObserved", "HarvestReport", "Observations", "xval", null, null, 0, 1, null);
            InsertDataIntoDatabase(connection, poDetails);

            DataTable poValues = TableFactory.CreateEmptyPredictedObservedValuesTable();
            poValues.Rows.Add(1, 1, "xval", 0.1, null, null, null, null, "GrainWt", 0.9, 1.1);
            poValues.Rows.Add(1, 2, "xval", 0.1, null, null, null, null, "GrainWt", 0.5, 1.0);
            InsertDataIntoDatabase(connection, poValues);

            DataTable poTests = TableFactory.CreateEmptyPredictedObservedTestsTable();
            poTests.Rows.Add(1, "GrainWt", "n", null, 2, null, 0, null, null, 0, null);
            poTests.Rows.Add(1, "GrainWt", "Slope", null, 4, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "Intercept", null, -3.5, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "SEslope", null, 0, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "SEintercept", null, 0, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "R2", null, 1, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "RMSE", null, 0.380789, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "NSE", null, -57, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "ME", null, -0.35, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "MAE", null, 0.35, null, 0, null, null, 1, null);
            poTests.Rows.Add(1, "GrainWt", "RSR", null, 5.385165, null, 0, null, null, 1, null);
            InsertDataIntoDatabase(connection, poTests);
        }

        private static void Initialise(SQLiteConnection connection)
        {
            string sql = ReflectionUtilities.GetResourceAsString("APSIM.PerformanceTests.Tests.CreateTables.sqlite");
            using (DbCommand command = connection.CreateCommand(sql))
                command.ExecuteNonQuery();
        }

        private static void Initialise(SqlConnection connection)
        {
            string sql = ReflectionUtilities.GetResourceAsString("APSIM.PerformanceTests.Tests.CreateTables.sqlserver");
            using (DbCommand command = connection.CreateCommand(sql))
                command.ExecuteNonQuery();
        }
    }
}
