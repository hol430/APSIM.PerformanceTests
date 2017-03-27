using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using APSIM.PerformanceTests.Portal.Models;
using System.IO;

namespace APSIM.PerformanceTests.Portal.DataAccessLayer
{
    public partial class ApsimDBContext : DbContext
    {
        public ApsimDBContext()
            : base(GetConnectionString())
        {
        }

        public DbSet<ApsimFile> ApsimFiles { get; set; }
        public DbSet<Simulation> Simulations { get; set; }
        public DbSet<PredictedObservedDetail> PredictedObservedDetails { get; set; }
        public DbSet<PredictedObservedValue> PredictedObservedValues { get; set; }


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


}}