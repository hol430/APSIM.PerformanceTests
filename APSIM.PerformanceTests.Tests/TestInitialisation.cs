using System;
using NUnit.Framework;
using APSIM.Shared.Utilities;

namespace APSIM.PerformanceTests.Tests
{
    [SetUpFixture]
    public class TestInitialisation
    {
        public static SQLite Connection { get; private set; }
        
        [OneTimeSetUp]
        public void CreateDatabase()
        {
            Connection = new SQLite();
            Connection.OpenDatabase(":memory:", readOnly: false);

            // Now create the tables.
            string sql = ReflectionUtilities.GetResourceAsString("APSIM.PerformanceTests.Tests.CreateTables.sql");
            Connection.ExecuteNonQuery(sql);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {

        }
    }
}
