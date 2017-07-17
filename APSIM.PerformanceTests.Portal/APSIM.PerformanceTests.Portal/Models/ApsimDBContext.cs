using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.IO;
using System.Web;


namespace APSIM.PerformanceTests.Portal.Models
{
    public partial class ApsimDBContext : DbContext
    {
        public ApsimDBContext()
            : base(GetConnectionString())
        {
        }

        //these are in the database
        public virtual DbSet<ApsimFile> ApsimFiles { get; set; }
        public virtual DbSet<PredictedObservedDetail> PredictedObservedDetails { get; set; }
        public virtual DbSet<PredictedObservedTest> PredictedObservedTests { get; set; }
        public virtual DbSet<PredictedObservedValue> PredictedObservedValues { get; set; }
        public virtual DbSet<Simulation> Simulations { get; set; }


        /// <summary>
        /// GEts the connection string for the database
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            string file = @"D:\Websites\dbConnect.txt";
            string connectionString = string.Empty;
#if DEBUG
            file = @"C:\Dev\PerformanceTests\dbConnect.txt";
#endif
            try
            {
                connectionString = File.ReadAllText(file) + ";Database=\"APSIM.PerformanceTests\"";
                return connectionString;

            }
            catch (Exception ex)
            {
                //WriteToLogFile("ERROR: Unable to retrieve Database connection details: " + ex.Message.ToString());
                connectionString = ex.Message.ToString();
                return connectionString;
            }
        }
    }
}