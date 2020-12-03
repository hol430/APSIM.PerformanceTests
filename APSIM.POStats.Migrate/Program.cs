using APSIM.POStats.Migrate.OldData;
using APSIM.POStats.Portal.Data;
using APSIM.POStats.Shared;
using APSIM.POStats.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MigrateData
{
    class Program
    {
        /// <summary>
        /// Migrates data from one database to another. Can move from old DB to new DB and using any connection string.
        /// Example usage to migrate from old SQL schema to new schema
        ///    APSIM.POStats.Migrate oldDbContext.txt newDbContext.txt PullRequestIds.txt
        /// Example to migrate from new SQL schema to new schema (one db to another)
        ///    APSIM.POStats.Migrate newDbContext1.txt newDbContext2.txt 
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>1 on error. 0 otherwise.</returns>
        static int Main(string[] args)
        {
            try
            {
                if (args.Length > 1)
                    throw new Exception("Usage: APSIM.POStats.Migrate [PRIdsFile]");

                var fromConnectionString = Vault.Read("OLDDB");
                var toConnectionString = Vault.Read("NEWDB");

                var pullRequestIds = new List<int>();
                if (args.Length == 1)
                {
                    if (!File.Exists(args[0]))
                        throw new Exception($"Cannot find file {args[0]}");
                    File.ReadAllText(args[0])
                        .Split('\n')
                        .ToList()
                        .ForEach(st => { if (st != string.Empty) pullRequestIds.Add(Convert.ToInt32(st)); });
                    MigrateDataOldToNew(pullRequestIds, fromConnectionString, toConnectionString);
                }
                else
                    MigrateDataNewToNew(fromConnectionString, toConnectionString);
            }
            catch (Exception err)
            {
                Console.WriteLine($"Error: {err.Message}");
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Migrate data from new SQL Server schema to new schema. 
        /// </summary>
        /// <param name="fromConnectionString"></param>
        /// <param name="toConnectionString"></param>
        private static void MigrateDataNewToNew(string sourceConnectionString, string destinationConnectionString)
        {
            // Open a db context to the source DB
            using var sourceDb = CreateNewDbContext(sourceConnectionString);

            // Open a db context to the destinationDB
            using var destinationDb = CreateNewDbContext(destinationConnectionString);

            // Copy all pull requests from source to destination db.
            foreach (var pullRequest in sourceDb.PullRequests)
            {
                destinationDb.PullRequests.Add(pullRequest);
                destinationDb.SaveChanges();
            }
        }

        /// <summary>
        /// Migrate data from old SQL Server schema to new schema.
        /// </summary>
        /// <param name="pullRequestIds">Ids of pull requests to migrate.</param>
        /// <param name="sourceConnectionString">Connection string of source db.</param>
        /// <param name="destinationConnectionString">Connection string of destination db.</param>
        private static void MigrateDataOldToNew(IList<int> pullRequestIds, string sourceConnectionString, string destinationConnectionString)
        {
            // Open a db context to the source DB
            using var sourceDb = CreateOldDbContext(sourceConnectionString);

            // Open a db context to the destinationDB
            using var destinationDb = CreateNewDbContext(destinationConnectionString);

            // Create the destination database if necessary.
            destinationDb.Database.EnsureCreated();

            // Loop through all pull requests in source and migrate them to destination.
            var stopWatch = Stopwatch.StartNew();
            int numDone = 0;
            bool isNewPR = false;
            foreach (var pullRequestId in pullRequestIds)
            {
                PullRequest newPullRequest = destinationDb.PullRequests.FirstOrDefault(pr => pr.Number == pullRequestId);
                if (newPullRequest == null)
                {
                    newPullRequest = new PullRequest();
                    newPullRequest.Number = pullRequestId;
                }
                else
                {
                    // Remove existing data.
                    newPullRequest.Files.Clear();
                    destinationDb.SaveChanges();
                }

                // See if this pull request was 'accepted'. If so then set the accept date in the new pull request.
                var sourceAcceptLog = sourceDb.AcceptStatsLogs.FirstOrDefault(a => a.PullRequestId == pullRequestId);
                if (sourceAcceptLog != null)
                    newPullRequest.DateStatsAccepted = sourceAcceptLog.LogAcceptDate;

                newPullRequest.Files = new List<ApsimFile>();
                Console.WriteLine($"Processing pull request {pullRequestId}");

                // Iterate through all matching pull request files in old DB.
                foreach (var file in sourceDb.ApsimFiles.Where(af => af.PullRequestId == pullRequestId))
                {
                    Console.WriteLine($"    Processing file {file.FileName}");

                    // The first time through here we need to set the author and daterun fields.
                    if (newPullRequest.Author == null)
                    {
                        newPullRequest.Author = file.SubmitDetails;
                        newPullRequest.DateRun = file.RunDate;
                    }

                    var newFile = new ApsimFile();
                    newFile.Name = file.FileName;
                    newFile.PullRequestId = newPullRequest.Id;
                    newFile.Tables = new List<Table>();

                    // Iterate through all the details records and create a 'Variable' instance
                    // for each variable.

                    foreach (var details in file.PredictedObservedDetails)
                    {
                        // Create a new table.
                        var newTable = new Table();
                        newTable.Name = details.TableName;
                        newTable.Variables = new List<Variable>();
                        newFile.Tables.Add(newTable);

                        // Read stats from old db. There are situations where stats are present but
                        // the predicted / observed data is not present - not sure why - bug in old code?
                        ReadPredictedObservedTests(sourceDb, newTable, details);

                        // Read predicted / observed data.
                        ReadVariableData(details, newTable);
                    }

                    newPullRequest.Files.Add(newFile);
                }

                if (isNewPR && newPullRequest != null && newPullRequest.Files.Count > 0)
                    destinationDb.PullRequests.Add(newPullRequest);

                // Save to database.
                destinationDb.SaveChanges();

                // Report progress.
                numDone++;
                int percentComplete = Convert.ToInt32(numDone * 1.0 / pullRequestIds.Count * 100);
                Console.WriteLine($"Percent complete: {percentComplete}. Time elapsed: {stopWatch.Elapsed.TotalMinutes} minutes.");
            }

            // Hook up accepted pull request ids
            foreach (var destinationPullRequest in destinationDb.PullRequests)
            {
                Console.WriteLine($"Checking pull request {destinationPullRequest.Number} for it's accepted PR.");

                if (destinationPullRequest.AcceptedPullRequestId == null)
                {
                    // See if the accepted pr id should have a value.
                    var sourcePullRequest = sourceDb.ApsimFiles.FirstOrDefault(f => f.PullRequestId == destinationPullRequest.Number);
                    if (sourcePullRequest.AcceptedPullRequestId != 0)
                    {
                        // This pull request in the source db has an accepted pull request so attach the destination pull request
                        // to the appropriate accepted pull request.

                        // Does the destination DB have the accepted PR?
                        var destinationAcceptedPullRequest = destinationDb.PullRequests.FirstOrDefault(pr => pr.Number == sourcePullRequest.AcceptedPullRequestId);
                        if (destinationAcceptedPullRequest == null)
                            Console.WriteLine($"ERROR: Pull request { destinationPullRequest.Number} is supposed to have an accepted pull request of {sourcePullRequest.AcceptedPullRequestId}. Cannot find this accepted pull request.");
                        else
                        {
                            Console.WriteLine($"Updating accepted PR in pull request {destinationPullRequest.Number}.");
                            destinationPullRequest.AcceptedPullRequestId = sourcePullRequest.AcceptedPullRequestId;
                        }
                    }
                }
            }
            destinationDb.SaveChanges();
        }

        /// <summary>
        /// Read in all stats.
        /// </summary>
        /// <remarks>
        /// PredictedObservedTests looks like this:
        /// Variable	    Test	    Accepted	Current
        /// BiomassWt       n	        9	        9
        /// BiomassWt       Slope	    0.844029	0.844029
        /// BiomassWt       Intercept	216.358988	216.3589
        /// BiomassWt       SEslope	    0.310942	0.310942
        /// BiomassWt       SEintercept	138.231511	138.2315
        /// BiomassWt       R2	        0.51281	    0.51281	
        /// BiomassWt       RMSE	    317.065809	317.0658
        /// BiomassWt       NSE	        0.02354	    0.02354	
        /// BiomassWt       ME	        168.367032	168.3670
        /// BiomassWt       MAE	        170.004948	170.0049
        /// BiomassWt       RSR	        0.931646	0.931646
        /// Distance	    n	        103	103	0	1	
        /// Distance	    Slope	    0.996537	0.996537
        /// Distance	    Intercept	0.70538	    0.70538	
        /// </remarks
        private static void ReadPredictedObservedTests(OldStatsDbContext sourceDb, Table newTable, APSIM.POStats.Migrate.OldModels.PredictedObservedDetails details)
        {
            Variable newVariable = null;
            foreach (var testRecord in sourceDb.PredictedObservedTests.Where(t => t.PredictedObservedDetailsID == details.ID)
                                                                .OrderBy(t => t.Variable))
            {
                // Determine if we need to create a new variable instance.
                if (newVariable == null || newVariable.Name != testRecord.Variable)
                {
                    // Yes we need to create a new variable. Do we need to save the existing one first?
                    newVariable = new Variable();
                    newVariable.Name = testRecord.Variable;
                    newVariable.Data = new List<VariableData>();
                    newTable.Variables.Add(newVariable);
                }
                if (testRecord.Current != null)
                {
                    if (testRecord.Test == "n")
                        newVariable.N = Convert.ToInt32(testRecord.Current);
                    else if (testRecord.Test == "RMSE")
                        newVariable.RMSE = (double) testRecord.Current;
                    else if (testRecord.Test == "NSE")
                        newVariable.NSE = (double)testRecord.Current;
                    else if (testRecord.Test == "RSR")
                        newVariable.RSR = (double)testRecord.Current;
                }
            }
        }

        /// <summary>
        /// Read data from all variables from 
        /// </summary>
        /// <remarks>
        /// e.g. of PredictedObservedValues table.
        /// SimulationsID	MatchName	MatchValue	MatchName2	MatchValue2	MatchName3	MatchValue3	ValueName	PredictedValue	ObservedValue
        /// 11082080	    Zone         TreeRow        Date	22/03/2004      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	28/05/2004      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	13/08/2004      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	17/09/2004      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	18/10/2004      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	24/11/2004      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	15/12/2004      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	13/01/2005      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	10/02/2005      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	15/03/2005      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	29/07/2005      NULL    NULL        Distance	0	                0
        /// 11082080	    Zone         TreeRow        Date	4/10/2005       NULL    NULL        Distance	0	                0
        /// 11082080	    Zone        	5m	        Date	22/03/2004      NULL    NULL        Distance	5	                5
        /// 11082080	    Zone        	5m	        Date	28/05/2004      NULL    NULL        Distance	5	                5
        /// 11082080	    Zone        	5m	        Date	13/08/2004      NULL    NULL        Distance	5	                5
        /// </remarks>
        /// <param name="details"></param>
        /// <param name="newTable"></param>
        private static void ReadVariableData(APSIM.POStats.Migrate.OldModels.PredictedObservedDetails details, Table newTable)
        {
            Variable variable = null;
            foreach (var value in details.PredictedObservedValues.Where(v => v.PredictedValue != null &&
                                                                             v.ObservedValue != null &&
                                                                             !double.IsNaN((double)v.PredictedValue) &&
                                                                             !double.IsNaN((double)v.ObservedValue))
                                                                 .OrderBy(v => v.ValueName))
            {
                // Determine if we need to create a new variable instance.
                if (variable == null || variable.Name != value.ValueName)
                {
                    if (variable != null)
                        VariableFunctions.EnsureStatsAreCalculated(variable);

                    variable = newTable.Variables.FirstOrDefault(v => v.Name == value.ValueName);
                    if (variable == null)
                        throw new Exception($"Cannot find variable {value.ValueName} while reading table {newTable.Name}");
                }

                // Create a label from simulation name and match data.
                string label = "Simulation: " + value.Simulations.Name;
                if (value.MatchName != null)
                    label += $", {value.MatchName}: {FormatValue(value.MatchValue)}";
                if (value.MatchName2 != null)
                    label += $", {value.MatchName2}: {FormatValue(value.MatchValue2)}";
                if (value.MatchName3 != null)
                    label += $", {value.MatchName3}: {FormatValue(value.MatchValue3)}";

                // Add a new predicted / observed data pair to the new variable.
                variable.Data.Add(new VariableData()
                {
                    Label = label,
                    Predicted = value.PredictedValue.Value,
                    Observed = value.ObservedValue.Value
                });
            }
            if (variable != null)
                VariableFunctions.EnsureStatsAreCalculated(variable);
        }

        /// <summary>
        /// Create a database context based on a connection string.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        private static StatsDbContext CreateNewDbContext(string connectionString)
        {
            var destinationDBOptions = new DbContextOptionsBuilder<StatsDbContext>();
            if (connectionString.Contains(".db"))
                destinationDBOptions.UseLazyLoadingProxies().UseSqlite(connectionString);
            else
                destinationDBOptions.UseLazyLoadingProxies().UseSqlServer(connectionString);

            return new StatsDbContext(destinationDBOptions.Options);
        }

        /// <summary>
        /// Create a database context based on a connection string.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        private static OldStatsDbContext CreateOldDbContext(string connectionString)
        {
            var destinationDBOptions = new DbContextOptionsBuilder<OldStatsDbContext>();
            if (connectionString.Contains(".db;"))
                destinationDBOptions.UseLazyLoadingProxies().UseSqlite(connectionString);
            else
                destinationDBOptions.UseLazyLoadingProxies().UseSqlServer(connectionString);

            return new OldStatsDbContext(destinationDBOptions.Options);
        }

        private static string FormatValue(string matchValue)
        {
            if (DateTime.TryParse(matchValue, out DateTime d))
                return d.ToString("yyyy-MM-dd");
            else
                return matchValue;
        }
    }
}
