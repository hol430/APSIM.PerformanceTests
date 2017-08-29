using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Data;
using System.Text;
using APSIM.Shared.Utilities;
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
                DataTable currentTable = new DataTable("StatTests");
                currentTable.Columns.Add("Variable", typeof(string));
                currentTable.Columns.Add("Test", typeof(string));
                currentTable.Columns.Add("Accepted", typeof(double));
                currentTable.Columns.Add("Current", typeof(double));
                currentTable.Columns.Add("Difference", typeof(double));
                currentTable.Columns.Add("PassedTest", typeof(string));
                currentTable.Columns.Add("AcceptedPredictedObservedTestsID", typeof(int));

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

                //Now add the comparison columns and determine values

                string sigIdent = "0";   //false   (1 = true)
                foreach (DataRow row in currentTable.Rows)
                {
                    //If we are starting from scratch, then set the Accepted the same as the Current.
                    if (acceptedStats.Rows.Count <= 0)
                    {
                        row["Accepted"] = row["Current"];
                    }

                    if (row["Accepted"] != DBNull.Value && row["Current"] != DBNull.Value)
                    {
                        row["Difference"] = Convert.ToDouble(row["Current"]) - Convert.ToDouble(row["Accepted"]);
                        row["PassedTest"] = Math.Abs(Convert.ToDouble(row["Difference"])) > Math.Abs(Convert.ToDouble(row["Accepted"])) * 0.01 ? sigIdent : "1";
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

                //Need to ensure that the order of the columns in the Datatable matches our table type
                currentTable.Columns["Variable"].SetOrdinal(0);
                currentTable.Columns["Test"].SetOrdinal(1);
                currentTable.Columns["Accepted"].SetOrdinal(2);
                currentTable.Columns["Current"].SetOrdinal(3);
                currentTable.Columns["Difference"].SetOrdinal(4);
                currentTable.Columns["PassedTest"].SetOrdinal(5);
                currentTable.Columns["AcceptedPredictedObservedTestsID"].SetOrdinal(6);

                return currentTable;
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  Unable to process Test Data: " + ex.Message.ToString());
            }

        }

    }
}