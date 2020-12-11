using APSIM.POStats.Shared.Models;
using APSIM.Shared.Utilities;
using System;

namespace APSIM.POStats.Shared
{
    /// <summary>
    /// Details about comparing two variables.
    /// </summary>
    public class VariableComparison
    {
        /// <summary>The current table.</summary>
        private readonly Variable current;

        /// <summary>The accepted file.</summary>
        private readonly Variable accepted;

        /// <summary>The possible comparison states.</summary>
        public enum Status
        {
            /// <summary>The stat for this variable is same as accepted stat (with tolerance).</summary>
            Same,

            /// <summary>The stat for this variable is NOT the same but is BETTER than accepted stat.</summary>
            Pass,

            /// <summary>The stat for this variable is NOT the same and is WORSE than accepted stat.</summary>
            Fail,

            /// <summary>This is a new variable (not in accepted).</summary>
            New,

            /// <summary>This is a missing variable in current (is in accepted).</summary>
            Missing,
        };

        /// <summary>Constructor</summary>
        public VariableComparison()
        {
        }

        /// <summary>Constructor</summary>
        public VariableComparison(Variable current, Variable accepted)
        {
            this.current = current;
            this.accepted = accepted;
            if (current != null && accepted != null)
            {
                NPercentDifference = CompareVariable(current.N, accepted.N, true);
                RMSEPercentDifference = CompareVariable(current.RMSE, accepted.RMSE, false);
                NSEPercentDifference = CompareVariable(current.NSE, accepted.NSE, true);
                RSRPercentDifference = CompareVariable(current.RSR, accepted.RSR, false);
            }
        }

        /// <summary>Name of variable.</summary>
        public string Name { get { if (current != null) return current.Name; else return accepted.Name; } }

        /// <summary>Id of variable.</summary>
        public int Id { get { if (current != null) return current.Id; else return accepted.Id; } }

        /// <summary>Number of observed data points in current.</summary>
        public int CurrentN => (current == null) ? int.MaxValue : current.N;
        
        /// <summary>RMSE of observed data points in current.</summary>
        public double CurrentRMSE => (current == null) ? double.NaN : current.RMSE;

        /// <summary>NSE of observed data points in current.</summary>
        public double CurrentNSE => (current == null) ? double.NaN : current.NSE;

        /// <summary>RSR of observed data points in current.</summary>
        public double CurrentRSR => (current == null) ? double.NaN : current.RSR;

        /// <summary>Number of observed data points in accepted.</summary>
        public int AcceptedN => (accepted == null) ? int.MaxValue : accepted.N;

        /// <summary>RMSE of observed data points in accepted.</summary>
        public double AcceptedRMSE => (accepted == null) ? double.NaN : accepted.RMSE;

        /// <summary>NSE of observed data points in accepted.</summary>
        public double AcceptedNSE => (accepted == null) ? double.NaN : accepted.NSE;

        /// <summary>RSR of observed data points in accepted.</summary>
        public double AcceptedRSR => (accepted == null) ? double.NaN : accepted.RSR;

        /// <summary>Return overall pass fail status.</summary>
        public bool IsPass => (NStatus == Status.Pass || NStatus == Status.Same) &&
                              (RMSEStatus == Status.Pass || RMSEStatus == Status.Same) &&
                              (NSEStatus == Status.Pass || NSEStatus == Status.Same) &&
                              (RSRStatus == Status.Pass || RSRStatus == Status.Same);

        /// <summary>Is this variable the same as the accepted variable?</summary>
        public bool IsSame => NStatus == Status.Same &&
                              RMSEStatus == Status.Same &&
                              NSEStatus == Status.Same &&
                              RSRStatus == Status.Same;

        /// <summary>How does current N compare to accepted N?</summary>
        public Status NStatus => CalculateState(NPercentDifference);
       
        /// <summary>How does current RMSE compare to accepted RMSE?</summary>
        public Status RMSEStatus => CalculateState(RMSEPercentDifference);

        /// <summary>How does current NSE compare to accepted NSE?</summary>
        public Status NSEStatus => CalculateState(NSEPercentDifference);

        /// <summary>How does current RSR compare to accepted RSR?</summary>
        public Status RSRStatus => CalculateState(RSRPercentDifference);

        /// <summary>N percentage difference between between the values.</summary>
        public double NPercentDifference { get; } = -double.NaN;

        /// <summary>RMSE percentage difference between between the values.</summary>
        public double RMSEPercentDifference { get; } = -double.NaN;

        /// <summary>NSE percentage difference between between the values.</summary>
        public double NSEPercentDifference { get; } = -double.NaN;

        /// <summary>RSR percentage difference between between the values.</summary>
        public double RSRPercentDifference { get; } = -double.NaN;

        
        /// <summary>
        /// Compare two stat variables and return percentage difference. If the difference is positive,
        /// it is an improvement else it is a decline.
        /// </summary>
        /// <param name="fromValue">The current pull request stat.</param>
        /// <param name="toValue">The accepted pull request stat.</param>
        /// <param name="positiveDiffIsGood">Is a positive difference (to - from) a good thing?</param>
        /// <returns></returns>
        private static double CompareVariable(double fromValue, double toValue, bool positiveDiffIsGood)
        {
            if (fromValue == toValue)
                return 0;

            fromValue = Math.Round(fromValue, 6);
            toValue = Math.Round(toValue, 6);
            var percentDifference = MathUtilities.Divide(toValue - fromValue, toValue, 0) * 100.0;

            // Correct the sign of the difference so that a positive value is an improvement and 
            // a negative value is a decline.
            if (!positiveDiffIsGood)
                percentDifference *= -1;
            
            return percentDifference;
        }

        /// <summary>
        /// Calculate a pass/fail state for a stat value based on percentage difference.
        /// </summary>
        /// <param name="percentDifference">The percent difference between from and to variables.</param>
        private Status CalculateState(double percentDifference)
        {
            if (current == null)
                return Status.Missing;
            else if (accepted == null)
                return Status.New;
            else if (double.IsNaN(percentDifference))
                return Status.Fail;
            else if (Math.Abs(percentDifference) <= 1)
                return Status.Same;
            else if (percentDifference < 0)
                return Status.Fail;
            else
                return Status.Pass;
        }
    }
}
