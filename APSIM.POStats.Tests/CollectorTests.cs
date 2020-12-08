using APSIM.Shared.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using APSIM.POStats.Shared;

namespace APSIM.POStats.Tests
{
    [TestFixture]
    public class CollectorTests
    {
        private string path;
        private SqliteConnection database;

        [SetUp]
        public void SetUp()
        {
            // Set the working directory to the unit test bin directory.
            var workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(workingDirectory);

            // Create and put temporary files in a temp directory.
            path = Path.Combine(Path.GetTempPath(), "Test");
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
            Directory.CreateDirectory(path);

            var filename = Path.Combine(path, "Test.apsimx");
            using (var writer = new FileStream(filename, FileMode.Create))
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("APSIM.POStats.Tests.Test.apsimx"))
                    if (stream != null)
                        stream.CopyTo(writer);

            // Create an empty database.
            var dbFileName = Path.Combine(path, "Test.db");
            database = new SqliteConnection($"Data source={dbFileName}");
            database.Open();
        }

        [TearDown]
        public void TearDown()
        {
            database.Close();
            Directory.Delete(path, recursive: true);
        }

        /// <summary>Ensures the collector works as expected.</summary>
        [Test]
        public void EnsureNormalCollectorOperationWorks()
        {
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("_Simulations",
                "ID,Name" + Environment.NewLine +
                " 1,Sim1" + Environment.NewLine
                ));

            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("PO1",
                "SimulationID,Date,Predicted.A,Observed.A" + Environment.NewLine +
                " 1,2000-01-01, 10.0, 11.0" + Environment.NewLine +
                " 1,2000-01-02, 20.0, 21.0" + Environment.NewLine
                ));

            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("PO2",
                "SimulationID,Date,Predicted.A,Observed.A" + Environment.NewLine +
                " 1,2000-01-03, 100.0, 110.0" + Environment.NewLine +
                " 1,2000-01-04, 200.0, 210.0" + Environment.NewLine
                ));
            database.Close();

            var pullRequest = Collector.RetrieveData(1234, new DateTime(2000, 1, 1), null, new string[] { path });
            Assert.AreEqual(1, pullRequest.Files.ToList().Count);
            Assert.AreEqual(2, pullRequest.Files[0].Tables.Count);

            // Table 1.
            Assert.AreEqual("PO1", pullRequest.Files[0].Tables[0].Name);
            Assert.AreEqual(1, pullRequest.Files[0].Tables[0].Variables.Count);
            Assert.AreEqual("A", pullRequest.Files[0].Tables[0].Variables[0].Name);
            Assert.AreEqual(2, pullRequest.Files[0].Tables[0].Variables[0].N);
            Assert.AreEqual(1, pullRequest.Files[0].Tables[0].Variables[0].RMSE);
            Assert.AreEqual(0.96, pullRequest.Files[0].Tables[0].Variables[0].NSE);
            Assert.AreEqual(0.1414213562373095, pullRequest.Files[0].Tables[0].Variables[0].RSR);
            Assert.AreEqual(2, pullRequest.Files[0].Tables[0].Variables[0].Data.Count);
            Assert.AreEqual(11.0, pullRequest.Files[0].Tables[0].Variables[0].Data[0].Observed);
            Assert.AreEqual(10.0, pullRequest.Files[0].Tables[0].Variables[0].Data[0].Predicted);
            Assert.AreEqual("Simulation: Sim1, Date: 2000-01-01", pullRequest.Files[0].Tables[0].Variables[0].Data[0].Label);
            Assert.AreEqual(21.0, pullRequest.Files[0].Tables[0].Variables[0].Data[1].Observed);
            Assert.AreEqual(20.0, pullRequest.Files[0].Tables[0].Variables[0].Data[1].Predicted);
            Assert.AreEqual("Simulation: Sim1, Date: 2000-01-02", pullRequest.Files[0].Tables[0].Variables[0].Data[1].Label);

            // Table 2.
            Assert.AreEqual("PO2", pullRequest.Files[0].Tables[1].Name);
            Assert.AreEqual(1, pullRequest.Files[0].Tables[1].Variables.Count);
            Assert.AreEqual("A", pullRequest.Files[0].Tables[1].Variables[0].Name);
            Assert.AreEqual(2, pullRequest.Files[0].Tables[1].Variables[0].N);
            Assert.AreEqual(10, pullRequest.Files[0].Tables[1].Variables[0].RMSE);
            Assert.AreEqual(0.96, pullRequest.Files[0].Tables[1].Variables[0].NSE);
            Assert.AreEqual(0.1414213562373095, pullRequest.Files[0].Tables[1].Variables[0].RSR);

            Assert.AreEqual(2, pullRequest.Files[0].Tables[1].Variables[0].Data.Count);
            Assert.AreEqual(110.0, pullRequest.Files[0].Tables[1].Variables[0].Data[0].Observed);
            Assert.AreEqual(100.0, pullRequest.Files[0].Tables[1].Variables[0].Data[0].Predicted);
            Assert.AreEqual("Simulation: Sim1", pullRequest.Files[0].Tables[1].Variables[0].Data[0].Label);
            Assert.AreEqual(210.0, pullRequest.Files[0].Tables[1].Variables[0].Data[1].Observed);
            Assert.AreEqual(200.0, pullRequest.Files[0].Tables[1].Variables[0].Data[1].Predicted);
            Assert.AreEqual("Simulation: Sim1", pullRequest.Files[0].Tables[1].Variables[0].Data[1].Label);
        }

        /// <summary>Ensures the collector doesn't find predicted columns that are string datatype.</summary>
        [Test]
        public void EnsurePredictedStringColumnsAreIgnored()
        {
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("_Simulations",
                "ID,Name" + Environment.NewLine +
                " 1,Sim1" + Environment.NewLine
                ));

            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("PO1",
                "SimulationID,Date,Predicted.A,Observed.A" + Environment.NewLine +
                " 1,2000-01-01, x, 11.0" + Environment.NewLine +
                " 1,2000-01-02, x, 21.0" + Environment.NewLine
                ));
            database.Close();

            var pullRequest = Collector.RetrieveData(1234, new DateTime(2000, 1, 1), null, new string[] { path });
            Assert.AreEqual(0, pullRequest.Files.ToList().Count);
        }

        /// <summary>Ensures the collector doesn't find observed columns that are string datatype.</summary>
        [Test]
        public void EnsureObservedStringColumnsAreIgnored()
        {
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("_Simulations",
                "ID,Name" + Environment.NewLine +
                " 1,Sim1" + Environment.NewLine
                ));
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("PO1",
                "SimulationID,Date,Predicted.A,Observed.A" + Environment.NewLine +
                " 1,2000-01-01, 10.0, x" + Environment.NewLine +
                " 1,2000-01-02, 20.0, x" + Environment.NewLine
                ));
            database.Close();

            var pullRequest = Collector.RetrieveData(1234, new DateTime(2000, 1, 1), null, new string[] { path });
            Assert.AreEqual(0, pullRequest.Files.ToList().Count);
        }

        /// <summary>Ensures the collector doesn't find observed columns that are string datatype.</summary>
        [Test]
        public void EnsureObservedStringValuesInRowsAreIgnored()
        {
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("_Simulations",
                "ID,Name" + Environment.NewLine +
                " 1,Sim1" + Environment.NewLine
                ));
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("PO1",
                "SimulationID,Date,Predicted.A,Observed.A" + Environment.NewLine +
                " 1,2000-01-01, 10.0, 11.0" + Environment.NewLine +
                " 1,2000-01-02, 20.0, x" + Environment.NewLine
                ));
            database.Close();

            var pullRequest = Collector.RetrieveData(1234, new DateTime(2000, 1, 1), null, new string[] { path });
            Assert.AreEqual(1, pullRequest.Files.ToList().Count);
            Assert.AreEqual(1, pullRequest.Files[0].Tables.Count);

            // Table 1.
            Assert.AreEqual("PO1", pullRequest.Files[0].Tables[0].Name);
            Assert.AreEqual(1, pullRequest.Files[0].Tables[0].Variables.Count);
            Assert.AreEqual("A", pullRequest.Files[0].Tables[0].Variables[0].Name);
            Assert.AreEqual(1, pullRequest.Files[0].Tables[0].Variables[0].Data.Count);
            Assert.AreEqual(11.0, pullRequest.Files[0].Tables[0].Variables[0].Data[0].Observed);
            Assert.AreEqual(10.0, pullRequest.Files[0].Tables[0].Variables[0].Data[0].Predicted);
        }

        /// <summary>Ensures the collector finds a predicted/observed table that isn't under the DataStore in the .apsimx file.</summary>
        [Test]
        public void EnsurePOTableNotUnderDataStoreIsFound()
        {
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("_Simulations",
                "ID,Name" + Environment.NewLine +
                " 1,Sim1" + Environment.NewLine
                ));
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("PO3",  // PO3 is not under DataStore in test.apsimx
                "SimulationID,Date,Predicted.A,Observed.A" + Environment.NewLine +
                " 1,2000-01-01, 10.0, 11.0" + Environment.NewLine +
                " 1,2000-01-02, 20.0, 21.0" + Environment.NewLine
                ));
            database.Close();

            var pullRequest = Collector.RetrieveData(1234, new DateTime(2000, 1, 1), null, new string[] { path });
            Assert.AreEqual(1, pullRequest.Files.ToList().Count);
            Assert.AreEqual(1, pullRequest.Files[0].Tables.Count);

            // Table 1.
            Assert.AreEqual("PO3", pullRequest.Files[0].Tables[0].Name);
            Assert.AreEqual(1, pullRequest.Files[0].Tables[0].Variables.Count);
            Assert.AreEqual("A", pullRequest.Files[0].Tables[0].Variables[0].Name);
            Assert.AreEqual(2, pullRequest.Files[0].Tables[0].Variables[0].Data.Count);
        }

        /// <summary>Ensures the collector will find integer predicted / observed numbers.</summary>
        [Test]
        public void EnsureCollectorFindsIntegers()
        {
            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("_Simulations",
                "ID,Name" + Environment.NewLine +
                " 1,Sim1" + Environment.NewLine
                ));

            SqliteUtilities.CreateTable(database, DataTableUtilities.FromCSV("PO1",
                "SimulationID,Date,Predicted.A,Observed.A" + Environment.NewLine +
                " 1,2000-01-01, 10, 11" + Environment.NewLine +
                " 1,2000-01-02, 20, 21" + Environment.NewLine
                ));
            database.Close();

            var pullRequest = Collector.RetrieveData(1234, new DateTime(2000, 1, 1), null, new string[] { path });
            Assert.AreEqual(1, pullRequest.Files.ToList().Count);
            Assert.AreEqual(1, pullRequest.Files[0].Tables.Count);

            // Table 1.
            Assert.AreEqual("PO1", pullRequest.Files[0].Tables[0].Name);
            Assert.AreEqual(1, pullRequest.Files[0].Tables[0].Variables.Count);
            Assert.AreEqual("A", pullRequest.Files[0].Tables[0].Variables[0].Name);
            Assert.AreEqual(2, pullRequest.Files[0].Tables[0].Variables[0].N);
            Assert.AreEqual(1, pullRequest.Files[0].Tables[0].Variables[0].RMSE);
            Assert.AreEqual(0.96, pullRequest.Files[0].Tables[0].Variables[0].NSE);
            Assert.AreEqual(0.1414213562373095, pullRequest.Files[0].Tables[0].Variables[0].RSR);
            Assert.AreEqual(2, pullRequest.Files[0].Tables[0].Variables[0].Data.Count);
            Assert.AreEqual(11.0, pullRequest.Files[0].Tables[0].Variables[0].Data[0].Observed);
            Assert.AreEqual(10.0, pullRequest.Files[0].Tables[0].Variables[0].Data[0].Predicted);
            Assert.AreEqual("Simulation: Sim1, Date: 2000-01-01", pullRequest.Files[0].Tables[0].Variables[0].Data[0].Label);
            Assert.AreEqual(21.0, pullRequest.Files[0].Tables[0].Variables[0].Data[1].Observed);
            Assert.AreEqual(20.0, pullRequest.Files[0].Tables[0].Variables[0].Data[1].Predicted);
            Assert.AreEqual("Simulation: Sim1, Date: 2000-01-02", pullRequest.Files[0].Tables[0].Variables[0].Data[1].Label);
        }

    }
}