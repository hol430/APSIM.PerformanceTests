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
                if (args.Length != 2 && args.Length != 3)
                    throw new Exception("Usage: APSIM.POStats.Migrate FromConnectionFile ToConnectionFile [PRIdsFile]");
                if (!File.Exists(args[0]))
                    throw new Exception($"Cannot find file {args[0]}");
                if (!File.Exists(args[1]))
                    throw new Exception($"Cannot find file {args[1]}");

                var fromConnectionString = File.ReadAllText(args[0]);
                var toConnectionString = File.ReadAllText(args[1]);

                var pullRequestIds = new List<int>();
                if (args.Length == 3)
                {
                    if (!File.Exists(args[2]))
                        throw new Exception($"Cannot find file {args[2]}");
                    File.ReadAllText(args[2])
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

            var stopWatch = Stopwatch.StartNew();
            int numDone = 0;
            foreach (var pullRequestId in pullRequestIds)
            {
                var newPullRequest = new PullRequest();
                newPullRequest.Id = pullRequestId;
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

                    // Iterate through all details records for the matching pull request file.
                    foreach (var details in file.PredictedObservedDetails)
                    {
                        var newTable = new Table();
                        newTable.Name = details.TableName;
                        newTable.Variables = new List<Variable>();

                        // Iterate through all the predicted / observed values for the details record.
                        Variable newVariable = null;
                        foreach (var value in details.PredictedObservedValues.Where(v => v.PredictedValue != null &&
                                                                                         v.ObservedValue != null &&
                                                                                         !double.IsNaN((double)v.PredictedValue) &&
                                                                                         !double.IsNaN((double)v.ObservedValue))
                                                                             .OrderBy(v => v.ValueName))
                        {
                            // Determine if we need to create a new variable instance.
                            if (newVariable == null || newVariable.Name != value.ValueName)
                            {
                                // Yes we need to create a new variable. Do we need to save the existing one first?
                                SaveVariable(newTable, newVariable);

                                newVariable = new Variable();
                                newVariable.Name = value.ValueName;
                                newVariable.Data = new List<VariableData>();
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
                            newVariable.Data.Add(new VariableData()
                            {
                                Label = label,
                                Predicted = value.PredictedValue.Value,
                                Observed = value.ObservedValue.Value
                            });
                        }
                        // Do we need to save the existing variable we were filling with data?
                        SaveVariable(newTable, newVariable);

                        newFile.Tables.Add(newTable);
                    }

                    newPullRequest.Files.Add(newFile);
                }
                if (newPullRequest != null && newPullRequest.Files.Count > 0)
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
                Console.WriteLine($"Checking pull request {destinationPullRequest.Id} for it's accepted PR.");

                if (destinationPullRequest.AcceptedPullRequestID == null)
                {
                    // See if the accepted pr id should have a value.
                    var sourcePullRequest = sourceDb.ApsimFiles.FirstOrDefault(f => f.PullRequestId == destinationPullRequest.Id);
                    if (sourcePullRequest.AcceptedPullRequestId != 0)
                    {
                        // This pull request in the source db has an accepted pull request so attach the destination pull request
                        // to the appropriate accepted pull request.

                        // Does the destination DB have the accepted PR?
                        var destinationAcceptedPullRequest = destinationDb.PullRequests.FirstOrDefault(pr => pr.Id == sourcePullRequest.AcceptedPullRequestId);
                        if (destinationAcceptedPullRequest == null)
                            Console.WriteLine($"ERROR: Pull request { destinationPullRequest.Id} is supposed to have an accepted pull request of {sourcePullRequest.AcceptedPullRequestId}. Cannot find this accepted pull request.");
                        else
                        {
                            Console.WriteLine($"Updating accepted PR in pull request {destinationPullRequest.Id}.");
                            destinationPullRequest.AcceptedPullRequestID = sourcePullRequest.AcceptedPullRequestId;
                        }
                    }
                }
            }
            destinationDb.SaveChanges();
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

        private static void SaveVariable(Table newTable, Variable newVariable)
        {
            if (newVariable != null)
            {
                VariableFunctions.EnsureStatsAreCalculated(newVariable);
                newTable.Variables.Add(newVariable);
            }
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
