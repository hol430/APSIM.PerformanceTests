using APSIM.POStats.Shared.Comparison;
using APSIM.POStats.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace APSIM.POStats.Shared
{
    public class PullRequestFunctions
    {
        /// <summary>
        /// Does the specified pull request pass?
        /// </summary>
        /// <param name="pullRequest"></param>
        /// <returns></returns>
        public static bool IsPass(PullRequest pullRequest)
        {
            foreach (var file in ApsimFileComparison.GetFiles(pullRequest))
            {
                if (file.Status != ApsimFileComparison.StatusType.NoChange)
                    return false;
                
                foreach (var table in file.GetTables())
                {
                    if (table.Status != ApsimFileComparison.StatusType.NoChange)
                        return false;
                    foreach (var variable in table.GetVariables())
                    {
                        if (!variable.IsPass)
                            return false;
                    }
                }
            }
            return true;
        }

    }
}
