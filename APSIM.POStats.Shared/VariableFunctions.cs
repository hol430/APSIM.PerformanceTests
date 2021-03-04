using APSIM.POStats.Shared.Models;
using APSIM.Shared.Utilities;
using System;

namespace APSIM.POStats.Shared
{
    /// <summary>
    /// Compares two variables.
    /// </summary>
    public class VariableFunctions
    {
        /// <summary>
        /// Ensure all stats are calculated if they aren't already.
        /// </summary>
        /// <param name="variable">The variable to calculate stats for.</param>
        public static void EnsureStatsAreCalculated(Variable variable, bool forceRecalculate = false)
        {
            if (forceRecalculate || (variable.N == 0 && variable.Data.Count > 0))
            {
                GetData(variable, out double[] predicted, out double[] observed, out _);
                var stats = MathUtilities.CalcRegressionStats(variable.Name, predicted, observed);
                if (!double.IsNaN(stats.n) && !double.IsInfinity(stats.n))
                    variable.N = stats.n;
                if (!double.IsNaN(stats.RMSE) && !double.IsInfinity(stats.RMSE))
                    variable.RMSE = stats.RMSE;
                if (!double.IsNaN(stats.NSE) && !double.IsInfinity(stats.NSE))
                    variable.NSE = stats.NSE;
                if (!double.IsNaN(stats.RSR) && !double.IsInfinity(stats.RSR))
                    variable.RSR = stats.RSR;
            }
        }

        /// <summary>
        /// For a given variable, get the associated accepted variable.
        /// </summary>
        /// <param name="variable">The variable to find accepted variable for.</param>
        /// <returns>Return accepted variable or null if not found.</returns>
        public static Variable GetAccepted(Variable variable)
        {
            var acceptedTable = FindAcceptedTable(variable.Table);
            if (acceptedTable != null)
                return acceptedTable.Variables.Find(v => v.Name == variable.Name);
            return null;
        }

        /// <summary>
        /// Return a string start rating (e.g. ***) for NSE
        /// </summary>
        /// <param name="nse">NSE value.</param>
        public static string NSERating(double nse)
        {
            if (nse > 0.75)
                return "***";
            else if (nse > 0.65)
                return "**";
            else if (nse > 0.50)
                return "*";
            else
                return string.Empty;
        }

        /// <summary>
        /// Return a string start rating (e.g. ***) for RSR
        /// </summary>
        /// <param name="rsr">RSR value.</param>
        public static string RSRRating(double rsr)
        {
            if (rsr <= 0.50)
                return "***";
            else if (rsr <= 0.60)
                return "**";
            else if (rsr <= 0.70)
                return "*";
            else
                return string.Empty;
        }

        /// <summary>
        /// Get all predicted and observed data.
        /// </summary>
        /// <param name="variable">The variable to get data for.</param>
        /// <param name="predicted">The returned predicted data.</param>
        /// <param name="observed">The returned observed data.</param>
        /// <param name="labels">Labels for each point.</param>
        public static void GetData(Variable variable, out double[] predicted, out double[] observed, out string[] labels)
        {
            if (variable != null)
            {
                predicted = new double[variable.Data.Count];
                observed = new double[variable.Data.Count];
                labels = new string[variable.Data.Count];
                for (int i = 0; i < variable.Data.Count; i++)
                {
                    predicted[i] = variable.Data[i].Predicted;
                    observed[i] = variable.Data[i].Observed;
                    labels[i] = variable.Data[i].Label;
                }
            }
            else
            {
                predicted = null;
                observed = null;
                labels = null;
            }
        }

        /// <summary>Compare two variables.</summary>
        /// <param name="currentVariable">The first variable.</param>
        /// <param name="acceptedVariable">The second variable.</param>
        public static VariableComparison Compare(Variable currentVariable, Variable acceptedVariable)
        {
            return new VariableComparison(currentVariable, acceptedVariable);
        }

        /// <summary>Searches for the corresponding accepted table.</summary>
        private static Table FindAcceptedTable(Table table)
        {
            var pullRequest = table.ApsimFile.PullRequest;
            if (pullRequest.AcceptedPullRequest != null)
            {
                var acceptedFile = pullRequest.AcceptedPullRequest.Files.Find(f => f.Name == table.ApsimFile.Name);
                if (acceptedFile != null)
                    return acceptedFile.Tables.Find(t => t.Name == table.Name);
            }
            return null;
        }
    }
}