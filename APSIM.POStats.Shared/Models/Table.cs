using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace APSIM.POStats.Shared.Models
{
    public class Table
    {
        private Table acceptedTable;
        private bool searchedForAcceptedTable = false;

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Variable> Variables { get; set; }

        [JsonIgnore]
        public int ApsimFileId { get; set; }

        [JsonIgnore]
        public virtual ApsimFile ApsimFile { get; set; }

        [JsonIgnore]
        public Table AcceptedTable
        {
            get
            {
                if (!searchedForAcceptedTable)
                    FindAcceptedTable();
                return acceptedTable;
            }
        }


        /// <summary>Searches for the corresponding accepted table.</summary>
        private void FindAcceptedTable()
        {
            searchedForAcceptedTable = true;

            var pullRequest = ApsimFile.PullRequest;
            if (pullRequest.AcceptedPullRequest != null)
            {
                var acceptedFile = pullRequest.AcceptedPullRequest.Files.Find(f => f.FileName == ApsimFile.FileName);
                if (acceptedFile != null)
                    acceptedTable = acceptedFile.Tables.Find(t => t.Name == Name);
            }
        }
    }
}
