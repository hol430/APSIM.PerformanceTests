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
            DataTable currentTable = CreateTestsStatsTable();
            string helperStr = string.Empty;
            try
            {
                MathUtilities.RegrStats[] stats;
                List<string> statNames = (new MathUtilities.RegrStats()).GetType().GetFields().Select(f => f.Name).ToList(); // use reflection, get names of stats available
                List<string> columnNames;

                Utilities.WriteToLogFile("    1/8. DoValidationTest: get the column names");
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
                Double xres, yres;

                Utilities.WriteToLogFile("    2/8. DoValidationTest: get the predicted observed values and calc regression stats");
                for (int c = 0; c < columnNames.Count; c++) //on each P/O column pair
                {
                    x.Clear();
                    y.Clear();
                    foreach (DataRow row in POtable.Rows)
                    {
                        xstr = row["Observed." + columnNames[c]].ToString();
                        ystr = row["Predicted." + columnNames[c]].ToString();
                        if ((Double.TryParse(xstr, out xres)) && (Double.TryParse(ystr, out yres)))
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
                Utilities.WriteToLogFile("    3/8. DoValidationTest: remove any null stats which can occur from non-numeric columns such as dates");
                List<MathUtilities.RegrStats> list = new List<MathUtilities.RegrStats>(stats);
                list.RemoveAll(l => l == null);
                stats = list.ToArray();

                //remove entries from column names
                Utilities.WriteToLogFile("    4/8. DoValidationTest: remove entries from column names");
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

                string variable, test, statValue;
                //Loop through stats and put them into a datatable
                Utilities.WriteToLogFile("    5/8. DoValidationTest: Loop through stats and put them into a datatable ");

                int rowIndex = 0;
                for (int i = 0; i < stats.Count(); i++)
                {
                    //for (int j = 1; j < statNames.Count; j++) //start at 1; we don't want Name field.
                    for (int j = 0; j < statNames.Count; j++) //need to ensure we don't do 'Name'
                    {
                        test = statNames[j];
                        if (test != "Name")
                        {
                            variable = stats[i].Name;
                            statValue = stats[i].GetType().GetField(statNames[j]).GetValue(stats[i]).ToString();
                            helperStr = "Variable: " + variable + ", Test: " + test + ", Value: " + statValue;

                            //CurrentTable.Rows.Add(PO_Name, stats[i].Name, statNames[j], null, current, null, null);
                            tRow = currentTable.NewRow();
                            tRow["Variable"] = variable;
                            tRow["Test"] = test;

                            hasValue = true;
                            try
                            {
                                current = Math.Round(Convert.ToDouble(statValue), 6);
                                if (double.IsNaN(current) == true) { hasValue = false; }
                                if (double.IsInfinity(current) == true) { hasValue = false; }
                                if (hasValue == true)
                                {
                                    tRow["Current"] = current;
                                    //currentTable.Rows[rowIndex]["Current"] = current;
                                }
                            }
                            catch (Exception)
                            {
                                Utilities.WriteToLogFile("    ERROR in DoValidationTest: Unable to convert:" + helperStr);
                            }

                            currentTable.Rows.Add(tRow);
                            rowIndex++;
                        }
                    }
                }
                helperStr = string.Empty;
                Utilities.WriteToLogFile("    6/8. DoValidationTest: Loop through stats and put them into a datatable - Completed");

                //Now merge this with out Accepted Table
                //Now add the comparison columns and determine values
                Utilities.WriteToLogFile("    7/8. DoValidationTest: MergeAndCompareAcceptedAgainstCurrent ");
                MergeAndCompareAcceptedAgainstCurrent(ref currentTable, acceptedStats);

                //Need to ensure that the order of the columns in the Datatable matches our table type
                Utilities.WriteToLogFile("    8/8. DoValidationTest: OrderCurrentTableforTableType ");
                OrderCurrentTableforTableType(ref currentTable);

                Utilities.WriteToLogFile("         DoValidationTest: complete");
            }
            catch (Exception ex)
            {
                //throw new Exception("ERROR in DoValidationTest:: " + ex.Message.ToString());
                Utilities.WriteToLogFile("    ERROR in DoValidationTest: " + helperStr + " - " + ex.Message.ToString());
            }
            return currentTable;
        }

        public static void MergeAndCompareAcceptedAgainstCurrent(ref DataTable currentTable, DataTable acceptedStats)
        {
            try
            {
                //Now merge this with out Accepted Table
                Utilities.WriteToLogFile("      1/4. MergeAndCompareAcceptedAgainstCurrent: get the column names");
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


                    Boolean rowsAdded = false;
                    Utilities.WriteToLogFile("      2/4. MergeAndCompareAcceptedAgainstCurrent: check 'Accepted' not in 'Current'");
                    //Need to check that there are no 'accepted' values that are not included in 'current stats'
                    foreach (DataRow rowAccepted in acceptedStats.Rows)
                    {
                        DataRow[] rowCurrent = currentTable.Select("Variable = '" + rowAccepted["Variable"] + "' AND Test = '" + rowAccepted["Test"] + "'");


                        //if the row doesn't exist in the current datatable, then need to add it.
                        if (rowCurrent.Count() == 0)
                        {
                            DataRow newRow = currentTable.NewRow();
                            newRow["Variable"] = rowAccepted["Variable"];
                            newRow["Test"] = rowAccepted["Test"];
                            newRow["Current"] = DBNull.Value;
                            newRow["Accepted"] = rowAccepted["Accepted"];
                            newRow["AcceptedPredictedObservedTestsID"] = rowAccepted["AcceptedPredictedObservedTestsID"];
                            currentTable.Rows.Add(newRow);

                            rowsAdded = true;
                        }
                    }
                    if (rowsAdded == true)
                    {
                        currentTable.AcceptChanges();

                        //TODO:  Need to re-sort the table
                        DataView dv = currentTable.DefaultView;
                        dv.Sort = "Variable, Test";
                        DataTable newTable = dv.ToTable();
                        currentTable = newTable;
                    }
                }



                bool convertOK;
                string sigIdent = "0";   //false   (1 = true)
                Utilities.WriteToLogFile("      3/4. MergeAndCompareAcceptedAgainstCurrent: evaluate difference'");
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

                        double currentValue, acceptedValue, diffValue;
                        convertOK = Double.TryParse(row["Current"].ToString(), out currentValue);
                        convertOK = Double.TryParse(row["Accepted"].ToString(), out acceptedValue);
                        diffValue = currentValue - acceptedValue;

                        row["Difference"] = diffValue;

                        //only do this for 'n'
                        if (row["Test"].ToString() == "n" && diffValue > 0)
                        {
                            row["PassedTest"] = false;
                        }
                        else
                        {
                            //row["PassedTest"] = Math.Abs(Convert.ToDouble(row["Difference"])) > Math.Abs(Convert.ToDouble(row["Accepted"])) * 0.01 ? sigIdent : "1";
                            row["PassedTest"] = Math.Abs(diffValue) > Math.Abs(acceptedValue) * 0.01 ? sigIdent : "1";
                        }

                        bool isImprovement = false;
                        switch (row["Test"].ToString())
                        {
                            case "R2":  //if the current is GREATER than accepted (ie difference is POSITIVE) then is an improvement
                                //if (row["Variable"].ToString() == "GrainWt")
                                //{
                                //    row["Variable"] = "GrainWt";
                                //}
                                if (currentValue > acceptedValue) { isImprovement = true; }
                                break;

                            case "RMSE":  //if the current is LESS than accepted (ie difference is NEGATIVE) then is an improvement
                                if (currentValue < acceptedValue) { isImprovement = true; }
                                break;

                            case "NSE":  //if the current value is closer to ZERO than the accepted , then it is an improvement
                                //if (Math.Abs(currentValue) < Math.Abs(acceptedValue)) { isImprovement = true; }
                                //modLMC - 15/11/2017 - change to this rule - after discussion with Dean (request from Hamish).
                                if (currentValue > acceptedValue) { isImprovement = true; }
                                break;

                            case "RSR":  //if the current is LESS than accepted (ie difference is NEGATIVE) then is an improvement
                                if (currentValue < acceptedValue) { isImprovement = true; }
                                break;
                        }
                        if (isImprovement == true)
                        {
                            row["PassedTest"] = "1";  //TRUE   (0 = false)
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
                Utilities.WriteToLogFile("      4/4. MergeAndCompareAcceptedAgainstCurrent: complete'");
            }
            catch (Exception ex)
            {
                //throw new Exception("    ERROR in MergeAndCompareAcceptedAgainstCurrent: " + ex.Message.ToString());
                Utilities.WriteToLogFile("    ERROR in MergeAndCompareAcceptedAgainstCurrent: " + ex.Message.ToString());
            }

        }

        public static DataTable CalculateStatsOnPredictedObservedValues(DataTable POtable)
        {
            DataTable currentTable = CreateTestsStatsTable();

            try
            {
                Utilities.WriteToLogFile("       1/4. CalculateStatsOnPredictedObservedValues:  Start processing");
                MathUtilities.RegrStats[] stats;
                List<string> statNames = (new MathUtilities.RegrStats()).GetType().GetFields().Select(f => f.Name).ToList(); // use reflection, get names of stats available

                var columnNames = (from row in POtable.AsEnumerable() select row.Field<string>("ValueName")).Distinct().ToList();

                //columnNames.Sort(); //ensure column names are always in the same order
                stats = new MathUtilities.RegrStats[columnNames.Count];

                List<double> x = new List<double>();
                List<double> y = new List<double>();
                string valueName = string.Empty, xstr, ystr;
                int c = 0;
                string holdValueName = POtable.Rows[0]["ValueName"].ToString();

                //loop through our current POtable and collate the necessary data to calculate stats for current PredictedObservedValues
                foreach (DataRow row in POtable.Rows) //on each P/O column pair
                {
                    valueName = row["ValueName"].ToString();
                    if (valueName != holdValueName)
                    {
                        if (x.Count != 0 || y.Count != 0)
                        {
                            stats[c] = MathUtilities.CalcRegressionStats(holdValueName, y, x);
                            c += 1;
                        }
                        holdValueName = valueName;
                        x.Clear();
                        y.Clear();
                    }

                    Double xres, yres;
                    xstr = row["ObservedValue"].ToString();
                    ystr = row["PredictedValue"].ToString();
                    if ((Double.TryParse(xstr, out xres)) && (Double.TryParse(ystr, out yres)))
                    {
                        x.Add(xres);
                        y.Add(yres);
                    }
                }
                Utilities.WriteToLogFile("       2/4. CalculateStatsOnPredictedObservedValues:  CalcRegressionStats");
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
                string variable, test, statValue, helperStr;

                Utilities.WriteToLogFile("       3/4. CalculateStatsOnPredictedObservedValues:  Loop through stats and put them into a datatable");
                //Loop through stats and put them into a datatable
                int rowIndex = 0;
                for (int i = 0; i < stats.Count(); i++)
                {
                    //for (int j = 1; j < statNames.Count; j++) //start at 1; we don't want Name field.
                    for (int j = 0; j < statNames.Count; j++) //need to ensure wthat we dont do 'Name' 
                    {
                        //current = Math.Round(Convert.ToDouble(stats[i].GetType().GetField(statNames[j]).GetValue(stats[i])), 6);
                        //CurrentTable.Rows.Add(PO_Name, stats[i].Name, statNames[j], null, current, null, null);
                        test = statNames[j];
                        if (test != "Name")
                        {
                            variable = stats[i].Name;
                            statValue = stats[i].GetType().GetField(statNames[j]).GetValue(stats[i]).ToString();
                            helperStr = "Variable: " + variable + ", Test: " + test + ", Value: " + statValue;

                            //CurrentTable.Rows.Add(PO_Name, stats[i].Name, statNames[j], null, current, null, null);
                            tRow = currentTable.NewRow();
                            tRow["Variable"] = variable;
                            tRow["Test"] = test;

                            hasValue = true;
                            try
                            {
                                current = Math.Round(Convert.ToDouble(statValue), 6);
                                if (double.IsNaN(current) == true) { hasValue = false; }
                                if (double.IsInfinity(current) == true) { hasValue = false; }
                                if (hasValue == true)
                                {
                                    tRow["Current"] = current;
                                    //currentTable.Rows[rowIndex]["Current"] = current;
                                }
                            }
                            catch (Exception)
                            {
                                Utilities.WriteToLogFile("    ERROR in DoValidationTest: Unable to convert:" + helperStr);
                            }
                            currentTable.Rows.Add(tRow);
                            rowIndex++;
                        }
                    }
                }
                Utilities.WriteToLogFile("       4/4. CalculateStatsOnPredictedObservedValues:  completed");
            }
            catch (Exception ex)
            {
                //throw new Exception("ERROR in CalculateStatsOnPredictedObservedValues:  Unable to process Test Data: " + ex.Message.ToString());
                Utilities.WriteToLogFile("     ERROR in CalculateStatsOnPredictedObservedValues:  Unable to process Test Data: " + ex.Message.ToString());
            }
            return currentTable;

        }

        public static DataTable MergeTestsStatsAndCompare(DataTable dtTests, DataTable acceptedStats)
        {
            try
            {
                //This is to ensure we have included the records where accepted stats exist, but current ones don't
                //acceptedStats.Columns.Add("Matched", typeof(string));

                Utilities.WriteToLogFile("       1/4. MergeTestsStatsAndCompare: started ");
                //Now merge this with out Accepted Table
                if (acceptedStats.Rows.Count > 0)
                {
                    foreach (DataRow rowCurrent in dtTests.Rows)
                    {
                        DataRow[] rowAccepted = acceptedStats.Select("Variable = '" + rowCurrent["Variable"] + "' AND Test = '" + rowCurrent["Test"] + "'");

                        if (rowAccepted.Count() == 0)
                        {
                            rowCurrent["Accepted"] = DBNull.Value;
                        }
                        else
                        {
                            rowCurrent["Accepted"] = rowAccepted[0]["Current"];
                        }
                    }

                    bool rowsAdded = false;
                    Utilities.WriteToLogFile("       2/4. MergeTestsStatsAndCompare: check for 'accepted' values not included in 'current stats' ");
                    //Need to check that there are no 'accepted' values that are not included in 'current stats'
                    foreach (DataRow rowAccepted in acceptedStats.Rows)
                    {
                        DataRow[] rowCurrent = dtTests.Select("Variable = '" + rowAccepted["Variable"] + "' AND Test = '" + rowAccepted["Test"] + "'");

                        //if the row doesn't exist in the current datatable, then need to add it.
                        if (rowCurrent.Count() == 0)
                        {
                            DataRow newRow = dtTests.NewRow();
                            newRow["Variable"] = rowAccepted["Variable"];
                            newRow["Test"] = rowAccepted["Test"];
                            newRow["Current"] = DBNull.Value;
                            newRow["Accepted"] = rowAccepted["Accepted"];
                            dtTests.Rows.Add(newRow);
                            rowsAdded = true;
                        }
                        dtTests.AcceptChanges();
                    }
                    if (rowsAdded == true)
                    {
                        dtTests.AcceptChanges();

                        //TODO:  Need to re-sort the table
                        DataView dv = dtTests.DefaultView;
                        dv.Sort = "Variable, Test";
                        DataTable newTable = dv.ToTable();
                        dtTests = newTable;
                    }

                }

                Utilities.WriteToLogFile("       3/4. MergeTestsStatsAndCompare: evaluate and compare ");
                bool convertOK;
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

                        double currentValue, acceptedValue, diffValue;
                        convertOK = Double.TryParse(row["Current"].ToString(), out currentValue);
                        convertOK = Double.TryParse(row["Accepted"].ToString(), out acceptedValue);
                        diffValue = currentValue - acceptedValue;

                        row["Difference"] = diffValue;

                        //only do this for 'n'
                        if (row["Test"].ToString() == "n" && diffValue > 0)
                        {
                            row["PassedTest"] = false;
                        }
                        else
                        {
                            row["PassedTest"] = Math.Abs(diffValue) > Math.Abs(acceptedValue) * 0.01 ? sigIdent : "1";
                        }

                        bool isImprovement = false;
                        switch (row["Test"].ToString())
                        {
                            case "R2":  //if the current is GREATER than accepted (ie difference is POSITIVE) then is an improvement
                                //if (row["Variable"].ToString() == "GrainWt")
                                //{
                                //    row["Variable"] = "GrainWt";
                                //}
                                if (currentValue > acceptedValue) { isImprovement = true; }
                                break;

                            case "RMSE":  //if the current is LESS than accepted (ie difference is NEGATIVE) then is an improvement
                                if (currentValue < acceptedValue) { isImprovement = true; }
                                break;

                            case "NSE":  //if the current value is closer to ZERO than the accepted , then it is an improvement
                                //if (Math.Abs(currentValue) < Math.Abs(acceptedValue)) { isImprovement = true; }
                                //modLMC - 15/11/2017 - change to this rule - after discussion with Dean (request from Hamish).
                                if (currentValue > acceptedValue) { isImprovement = true; }
                                break;

                            case "RSR":  //if the current is LESS than accepted (ie difference is NEGATIVE) then is an improvement
                                if (currentValue < acceptedValue) { isImprovement = true; }
                                break;
                        }
                        if (isImprovement == true)
                        {
                            row["PassedTest"] = "1";   //TRUE   (0 = false)
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
                Utilities.WriteToLogFile("       4/4. MergeTestsStatsAndCompare: completed ");
            }
            catch (Exception ex)
            {
                //throw new Exception("ERROR in MergeTestsStatsAndCompare:  " + ex.Message.ToString());
                Utilities.WriteToLogFile("     ERROR in MergeTestsStatsAndCompare:  " + ex.Message.ToString());
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