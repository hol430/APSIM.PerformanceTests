using APSIM.POStats.Shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace APSIM.POStats.Shared.Comparison
{
    public class ApsimFileComparison
    {
        /// <summary>The current file.</summary>
        private readonly ApsimFile current = null;

        /// <summary>The accepted file.</summary>
        private readonly ApsimFile accepted = null;

        /// <summary>Constructor.</summary>
        public ApsimFileComparison(ApsimFile currentFile, ApsimFile acceptedFile)
        {
            current = currentFile;
            accepted = acceptedFile;
        }

        /// <summary>Name of file.</summary>
        public string Name { get { if (current != null) return current.Name; else return accepted.Name; } }

        public enum StatusType
        {
            /// <summary>This is a new file (not in accepted).</summary>
            New,

            /// <summary>This is a missing file in current (is in accepted).</summary>
            Missing,

            /// <summary>File exists in both current and excepted.</summary>
            NoChange
        }

        /// <summary>Is this a new file or a missing file or the same file?</summary>
        public StatusType Status 
        { 
            get 
            { 
                if (current == null) 
                    return StatusType.Missing; 
                else if (accepted == null) return StatusType.New; 
                else 
                    return StatusType.NoChange; 
            } 
        }

        /// <summary>Get a list of all tables for this file.</summary>
        public IEnumerable<TableComparison> GetTables()
        {
            var tables = new List<TableComparison>();
            if (current != null)
            {
                foreach (var currentTable in current.Tables)
                {
                    var acceptedTable = accepted?.Tables.Find(t => t.Name == currentTable.Name);
                    tables.Add(new TableComparison(currentTable, acceptedTable));
                }

                // Add in tables that are in the accepted file but not in the current file.
                if (accepted != null)
                {
                    var tablesNotInCurrent = accepted.Tables.Except(tables.Select(t => t.Accepted));
                    foreach (var acceptedTable in tablesNotInCurrent)
                        tables.Add(new TableComparison(null, acceptedTable));
                }
            }

            return tables.OrderBy(t => t.Name);
        }

        /// <summary>Get a list of all files for a pull request.</summary>
        /// <param name="pullRequest">The pull request.</param>
        public static IEnumerable<ApsimFileComparison> GetFiles(PullRequest pullRequest)
        {
            var files = new List<ApsimFileComparison>();
            foreach (var currentFile in pullRequest.Files)
            {
                var acceptedFile = pullRequest.AcceptedPullRequest?.Files.Find(f => f.Name == currentFile.Name);
                files.Add(new ApsimFileComparison(currentFile, acceptedFile));
            }

            // Add in files that are in the accepted PR but not in the current PR.
            if (pullRequest.AcceptedPullRequest != null)
            {
                var filesNotInCurrent = pullRequest.AcceptedPullRequest.Files.Except(files.Select(f => f.accepted));
                foreach (var acceptedFile in filesNotInCurrent)
                    files.Add(new ApsimFileComparison(null, acceptedFile));
            }

            return files.OrderBy(f => f.Name);
        }
    }
}