using APSIM.Shared.Utilities;
using APSIM.PerformanceTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Data;
using System.Text;
using System.ComponentModel;


namespace APSIM.PerformanceTests.Service
{
    public class Tests
    {
        /// <summary>
        /// Run tests
        /// </summary>
        public static DataTable DoValidationTest(string PO_Name, DataTable POtable, DataTable acceptedStats)
        {
            try
            {
                DataTable currentTable = CreateTestsStatsTable();

                MathUtilities.RegrStats[] stats;
                List<string> statNames = (new MathUtilities.RegrStats()).GetType().GetFields().Select(f => f.Name).ToList(); // use reflection, get names of stats available
                List<string> columnNames;

                columnNames = POtable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList(); //get list of column names
                columnNames = columnNames.Where(c => c.Contains("Observed")).ToList(); //filter names that are not pred/obs pairs
                for (int i = 0; i < columnNames.Count; i++)
                {
                    columnNames[i] = columnNames[i].Replace("Observed.", "");
                }

                columnNames.Sort(); //ensure column names are always in the same order
                stats = new MathUtilities.RegrStats[columnNames.Count];
                List<double> x = new List<double>();
                List<double> y = new List<double>();
                string xstr, ystr;
                double xres;
                double yres;

                for (int c = 0; c < columnNames.Count; c++) //on each P/O column pair
                {
                    x.Clear();
                    y.Clear();
                    foreach (DataRow row in POtable.Rows)
                    {
                        xstr = row["Observed." + columnNames[c]].ToString();
                        ystr = row["Predicted." + columnNames[c]].ToString();
                        if (Double.TryParse(xstr, out xres) && Double.TryParse(ystr, out yres))
                        {
                            x.Add(xres);
                            y.Add(yres);
                        }
                    }
                    if (x.Count == 0 || y.Count == 0)
                        continue;

                    stats[c] = MathUtilities.CalcRegressionStats(columnNames[c], y, x);
                }

                //remove any null stats which can occur from non-numeric columns such as dates
                List<MathUtilities.RegrStats> list = new List<MathUtilities.RegrStats>(stats);
                list.RemoveAll(l => l == null);
                stats = list.ToArray();

                //remove entries from column names
                for (int i = columnNames.Count() - 1; i >= 0; i--)
                {
                    bool found = false;
                    for (int j = 0; j < stats.Count(); j++)
                    {
                        if (columnNames[i] == stats[j].Name)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        columnNames.RemoveAt(i);
                }

                double current;
                DataRow tRow;
                bool hasValue;

                //Loop through stats and put them into a datatable
                int rowIndex = 0;
                for (int i = 0; i < stats.Count(); i++)
                {
                    for (int j = 1; j < statNames.Count; j++) //start at 1; we don't want Name field.
                    {
                        current = Math.Round(Convert.ToDouble(stats[i].GetType().GetField(statNames[j]).GetValue(stats[i])),6);
                        //CurrentTable.Rows.Add(PO_Name, stats[i].Name, statNames[j], null, current, null, null);
                        tRow = currentTable.NewRow();
                        tRow["Variable"] = stats[i].Name;
                        tRow["Test"] = statNames[j];

                        hasValue = true;
                        if (double.IsNaN(current) == true) { hasValue = false; }
                        if (double.IsInfinity(current) == true) { hasValue = false; }
                        if (hasValue == true)
                        {
                            tRow["Current"] = current;
                            //currentTable.Rows[rowIndex]["Current"] = current;
                        }
                        currentTable.Rows.Add(tRow);
                        rowIndex++;
                    }
                }

                //Now merge this with out Accepted Table
                //Now add the comparison columns and determine values
                MergeAndCompareAcceptedAgainstCurrent(ref currentTable, acceptedStats);

                //Need to ensure that the order of the columns in the Datatable matches our table type
                OrderCurrentTableforTableType(ref currentTable);

                return currentTable;
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  Unable to process Test Data: " + ex.Message.ToString());
            }

        }

        public static void MergeAndCompareAcceptedAgainstCurrent(ref DataTable currentTable, DataTable acceptedStats)
        {
            //Now merge this with out Accepted Table
            if (acceptedStats.Rows.Count > 0)
            {
                foreach (DataRow rowCurrent in currentTable.Rows)
                {
                    DataRow[] rowAccepted = acceptedStats.Select("Variable = '" + rowCurrent["Variable"] + "' AND Test = '" + rowCurrent["Test"] + "'");

                    if (rowAccepted.Count() == 0)
                    {
                        rowCurrent["Accepted"] = DBNull.Value;
                        rowCurrent["AcceptedPredictedObservedTestsID"] = DBNull.Value; ;
                    }
                    else
                    {
                        rowCurrent["Accepted"] = rowAccepted[0]["Accepted"];
                        rowCurrent["AcceptedPredictedObservedTestsID"] = rowAccepted[0]["AcceptedPredictedObservedTestsID"];
                    }
                }
            }

            string sigIdent = "0";   //false   (1 = true)
            foreach (DataRow row in currentTable.Rows)
            {
                //If we are starting from scratch, then set the Accepted the same as the Current.
                if (acceptedStats.Rows.Count <= 0)
                {
                    // 30/08/2017 - modLMC - originally this was updated when there were not Accepted Stats (ie, first time being saved), however
                    // now don't save anything to see how it will work (as per discussion with Dean Holzworth).
                    //row["Accepted"] = row["Current"];
                    row["Accepted"] = DBNull.Value;
                }

                if (row["Accepted"] != DBNull.Value && row["Current"] != DBNull.Value)
                {
                    double currentValue = Convert.ToDouble(row["Current"]);
                    double acceptedValue = Convert.ToDouble(row["Accepted"]);

                    row["Difference"] = currentValue - acceptedValue;
                    row["PassedTest"] = Math.Abs(Convert.ToDouble(row["Difference"])) > Math.Abs(Convert.ToDouble(row["Accepted"])) * 0.01 ? sigIdent : "1";

                    bool isImprovement = false;
                    switch (row["Test"].ToString())
                    {
                        case "R2":  //if the current is GREATER than accepted (ie difference is POSITIVE) then is an improvement
                            if (currentValue < acceptedValue) isImprovement = true;
                            break;

                        case "RMSE":  //if the current is LESS than accepted (ie difference is NEGATIVE) then is an improvement
                            if (currentValue > acceptedValue) isImprovement = true;
                            break;

                        case "NSE":  //if the current value is closer to ZERO than the accepted , then it is an improvement
                            if (Math.Abs(currentValue) < Math.Abs(acceptedValue)) isImprovement = true;
                            break;

                        case "RSR":  //if the current is LESS than accepted (ie difference is NEGATIVE) then is an improvement
                            if (currentValue < acceptedValue) isImprovement = true;
                            break;
                    }
                    if (isImprovement == true)
                    {
                        row["PassedTest"] = true;
                    }
                    //Always update this
                    row["IsImprovement"] = isImprovement;

                }
                else if (row["Accepted"] == DBNull.Value && row["Current"] == DBNull.Value)
                {
                    //if the tests are both null, ie where n=1 and the other stats don't calculate, then make sure that we dont update the passed tests value.
                    row["PassedTest"] = DBNull.Value;
                }
                else
                {
                    row["Difference"] = DBNull.Value;
                    row["PassedTest"] = sigIdent;
                }
            }

        }

        public static DataTable CalculateStatsOnPredictedObservedValues(DataTable POtable)
        {
            try
            {
                DataTable currentTable = CreateTestsStatsTable();

                MathUtilities.RegrStats[] stats;
                List<string> statNames = (new MathUtilities.RegrStats()).GetType().GetFields().Select(f => f.Name).ToList(); // use reflection, get names of stats available

                var columnNames = (from row in POtable.AsEnumerable() select row.Field<string>("ValueName")).Distinct().ToList();

                //columnNames.Sort(); //ensure column names are always in the same order
                stats = new MathUtilities.RegrStats[columnNames.Count];

                List<double> x = new List<double>();
                List<double> y = new List<double>();
                string valueName = string.Empty, xstr, ystr;
                double xres;
                double yres;
                int c = 0;
                string holdValueName = POtable.Rows[0]["ValueName"].ToString();

                //loop through our current POtable and collate the necessary data to calculate stats for current PredictedObservedValues
                foreach (DataRow row in POtable.Rows) //on each P/O column pair
                {
                    valueName = row["ValueName"].ToString();
                    if ((valueName != holdValueName) && (x.Count != 0 || y.Count != 0))
                    {
                        stats[c] = MathUtilities.CalcRegressionStats(holdValueName, y, x);
                        holdValueName = valueName;
                        x.Clear();
                        y.Clear();
                        c += 1;
                    }

                    xstr = row["ObservedValue"].ToString();
                    ystr = row["PredictedValue"].ToString();
                    if (Double.TryParse(xstr, out xres) && Double.TryParse(ystr, out yres))
                    {
                        x.Add(xres);
                        y.Add(yres);
                    }
                }
                if (x.Count != 0 || y.Count != 0)
                {
                    stats[c] = MathUtilities.CalcRegressionStats(holdValueName, y, x);
                }


                //remove any null stats which can occur from non-numeric columns such as dates
                List<MathUtilities.RegrStats> list = new List<MathUtilities.RegrStats>(stats);
                list.RemoveAll(l => l == null);
                stats = list.ToArray();

                //remove entries from column names
                for (int i = columnNames.Count() - 1; i >= 0; i--)
                {
                    bool found = false;
                    for (int j = 0; j < stats.Count(); j++)
                    {
                        if (columnNames[i].ToString() == stats[j].Name)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        columnNames.RemoveAt(i);
                }

                double current;
                DataRow tRow;
                bool hasValue;

                //Loop through stats and put them into a datatable
                int rowIndex = 0;
                for (int i = 0; i < stats.Count(); i++)
                {
                    for (int j = 1; j < statNames.Count; j++) //start at 1; we don't want Name field.
                    {
                        current = Math.Round(Convert.ToDouble(stats[i].GetType().GetField(statNames[j]).GetValue(stats[i])), 6);
                        //CurrentTable.Rows.Add(PO_Name, stats[i].Name, statNames[j], null, current, null, null);
                        tRow = currentTable.NewRow();
                        tRow["Variable"] = stats[i].Name;
                        tRow["Test"] = statNames[j];

                        hasValue = true;
                        if (double.IsNaN(current) == true) { hasValue = false; }
                        if (double.IsInfinity(current) == true) { hasValue = false; }
                        if (hasValue == true)
                        {
                            tRow["Current"] = current;
                            //currentTable.Rows[rowIndex]["Current"] = current;
                        }
                        currentTable.Rows.Add(tRow);
                        rowIndex++;
                    }
                }

                return currentTable;
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  Unable to process Test Data: " + ex.Message.ToString());
            }

        }

        public static DataTable MergeTestsStatsAndCompare(DataTable dtTests, DataTable acceptedStats)
        {
            try
            {
                //This is to ensure we have included the records where accepted stats exist, but current ones don't
                acceptedStats.Columns.Add("Matched", typeof(string));

                //Now merge this with out Accepted Table
                if (acceptedStats.Rows.Count > 0)
                {
                    foreach (DataRow rowCurrent in dtTests.Rows)
                    {
                        DataRow[] rowAccepted = acceptedStats.Select("Variable = '" + rowCurrent["Variable"] + "' AND Test = '" + rowCurrent["Test"] + "'");

                        if (rowAccepted.Count() == 0)
                        {
                            rowCurrent["Accepted"] = DBNull.Value;
                            //rowCurrent["AcceptedPredictedObservedTestsID"] = DBNull.Value; ;
                        }
                        else
                        {
                            rowCurrent["Accepted"] = rowAccepted[0]["Current"];
                            //rowCurrent["AcceptedPredictedObservedTestsID"] = rowAccepted[0]["ID"];

                            //Need to know if something was in accepted, but is not in current
                            rowAccepted[0]["Matched"] = "true";
                        }

                    }
                }

                //NOT SURE IF THIS IS REQUIRED OR NOT
                //DataRow newRow;
                //foreach (DataRow acceptedRow in acceptedStats.Rows)
                //{
                //    //Should be either null or true
                //    if ((string)acceptedRow["Matched"] != "true")
                //    {
                //        newRow = dtTests.NewRow();
                //        newRow["Variable"] = acceptedRow["Variable"];
                //        newRow["Test"] = acceptedRow["Test"];
                //        newRow["Accepted"] = acceptedRow["Current"];
                //        //newRow["AcceptedPredictedObservedTestsID"] = acceptedRow["Current"];
                //        dtTests.Rows.Add(newRow);
                //    }
                //}

                string sigIdent = "0";   //false   (1 = true)
                foreach (DataRow row in dtTests.Rows)
                {
                    //If we are starting from scratch, then set the Accepted the same as the Current.
                    if (acceptedStats.Rows.Count <= 0)
                    {
                        // 30/08/2017 - modLMC - originally this was updated when there were not Accepted Stats (ie, first time being saved), however
                        // now don't save anything to see how it will work (as per discussion with Dean Holzworth).
                        //row["Accepted"] = row["Current"];
                        row["Accepted"] = DBNull.Value;
                    }

                    if (row["Accepted"] != DBNull.Value && row["Current"] != DBNull.Value)
                    {
                        double currentValue = Convert.ToDouble(row["Current"]);
                        double acceptedValue = Convert.ToDouble(row["Accepted"]);

                        row["Difference"] = currentValue - acceptedValue;
                        row["PassedTest"] = Math.Abs(Convert.ToDouble(row["Difference"])) > Math.Abs(Convert.ToDouble(row["Accepted"])) * 0.01 ? sigIdent : "1";

                        bool isImprovement = false;
                        switch (row["Test"].ToString())
                        {
                            case "R2":  //if the current is GREATER than accepted (ie difference is POSITIVE) then is an improvement
                                if (currentValue < acceptedValue) isImprovement = true;
                                break;

                            case "RMSE":  //if the current is LESS than accepted (ie difference is NEGATIVE) then is an improvement
                                if (currentValue > acceptedValue) isImprovement = true;
                                break;

                            case "NSE":  //if the current value is closer to ZERO than the accepted , then it is an improvement
                                if (Math.Abs(currentValue) < Math.Abs(acceptedValue)) isImprovement = true;
                                break;

                            case "RSR":  //if the current is LESS than accepted (ie difference is NEGATIVE) then is an improvement
                                if (currentValue < acceptedValue) isImprovement = true;
                                break;
                        }
                        if (isImprovement == true)
                        {
                            row["PassedTest"] = true;
                        }
                        //Always update this
                        row["IsImprovement"] = isImprovement;

                    }
                    else if (row["Accepted"] == DBNull.Value && row["Current"] == DBNull.Value)
                    {
                        //if the tests are both null, ie where n=1 and the other stats don't calculate, then make sure that we dont update the passed tests value.
                        row["PassedTest"] = DBNull.Value;
                    }
                    else
                    {
                        row["Difference"] = DBNull.Value;
                        row["PassedTest"] = sigIdent;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to merge Current and Accepted Stats:  " + ex.Message.ToString());
            }
            return dtTests;
        }


        private static DataTable CreateTestsStatsTable()
        {
            DataTable table = new DataTable("StatTests");

            table.Columns.Add("Variable", typeof(string));
            table.Columns.Add("Test", typeof(string));
            table.Columns.Add("Accepted", typeof(double));
            table.Columns.Add("Current", typeof(double));
            table.Columns.Add("Difference", typeof(double));
            table.Columns.Add("PassedTest", typeof(string));
            table.Columns.Add("AcceptedPredictedObservedTestsID", typeof(int));
            table.Columns.Add("IsImprovement", typeof(bool));

            return table;
        }


        private static void OrderCurrentTableforTableType(ref DataTable table)
        {
            //Need to ensure that the order of the columns in the Datatable matches our table type
            table.Columns["Variable"].SetOrdinal(0);
            table.Columns["Test"].SetOrdinal(1);
            table.Columns["Accepted"].SetOrdinal(2);
            table.Columns["Current"].SetOrdinal(3);
            table.Columns["Difference"].SetOrdinal(4);
            table.Columns["PassedTest"].SetOrdinal(5);
            table.Columns["AcceptedPredictedObservedTestsID"].SetOrdinal(6);
            table.Columns["IsImprovement"].SetOrdinal(7);
        }

    }
}