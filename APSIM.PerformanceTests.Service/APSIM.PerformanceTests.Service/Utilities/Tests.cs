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
        /// A collection of validated stats.
        /// </summary>
        //[APSIM.Shared.Soils.Description("An array of validated regression stats.")]
        //public static MathUtilities.RegrStats[] acceptedStats { get; set; }

        /// <summary>
        /// A string containing the names of stats in the accepted values.
        /// Used for checking if the stats class has changed.
        /// </summary>
        //public static string AcceptedStatsName { get; set; }

        /// <summary>
        /// The name of the associated Predicted Observed node.
        /// </summary>
        //public static string POName { get; set; }

        /// <summary>
        /// Run tests
        /// </summary>
        public static DataTable DoValidationTest(string PO_Name, DataTable POtable, DataTable acceptedStats)
        {
            try
            {
                //PredictedObserved PO = Parent as PredictedObserved;
                //if (PO == null)
                //    return;
                //DataStore DS = PO.Parent as DataStore;
                DataTable currentTable = new DataTable("StatTests");
                currentTable.Columns.Add("Variable", typeof(string));
                currentTable.Columns.Add("Test", typeof(string));
                //currentTable.Columns.Add("Accepted", typeof(double));
                currentTable.Columns.Add("Current", typeof(double));
                //currentTable.Columns.Add("AcceptedPredictedObservedTestsID", typeof(int));


                MathUtilities.RegrStats[] stats;
                List<string> statNames = (new MathUtilities.RegrStats()).GetType().GetFields().Select(f => f.Name).ToList(); // use reflection, get names of stats available
                //DataTable POtable = DS.GetData("*", PO.Name);
                List<string> columnNames;
                string sigIdent = "0";   //false   (1 = true)

                if (POtable == null)
                {
                    //object sim = PO.Parent;
                    //while (sim as Simulations == null)
                    //    sim = ((Model)sim).Parent;

                    //throw new ApsimXException(this, "Could not find PO table in " + (sim != null ? ((Simulations)sim).FileName : "<unknown>") + ". Has the simulation been run?");
                }

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

                //turn stats array into a DataTable
                //first, check if there is already an AcceptedStats array, create if not.
                //If the names don't match, then use current stats as user has dragged
                //an already existing Test to a new node.
                //if (acceptedStats == null)
                //{
                //    throw new Exception("Tests Error:  Accepted Stats do not exist for this.");
                //    acceptedStats = stats;
                //    AcceptedStatsName = StringUtilities.Build(statNames, " ");
                //}

                //then make sure the names and order of the accepted stats are the same as the new ones.
                //if (StringUtilities.Build(statNames, " ") != AcceptedStatsName)
                //{
                //    throw new Exception("Tests Error:  Names, number or order of accepted stats do not match class MathUtilities.RegrStats. The class has probably changed.");
                //}

                //double accepted;
                double current;

                //DataTable AcceptedTable = Table.Copy();
                //DataTable CurrentTable = currentTable.Copy();

                DataRow tRow;
                bool hasValue;
                ////accepted table
                //for (int i = 0; i < acceptedStats.Count(); i++)
                //{
                //    for (int j = 1; j < statNames.Count; j++) //start at 1; we don't want Name field.
                //    {
                //        accepted = Math.Round(Convert.ToDouble(acceptedStats[i].GetType().GetField(statNames[j]).GetValue(acceptedStats[i])),6);
                //        //AcceptedTable.Rows.Add(PO_Name, AcceptedStats[i].Name, statNames[j],  accepted, null, null, null);

                //        tRow = AcceptedTable.NewRow();
                //        tRow["Name"] = PO_Name;
                //        tRow["Variable"] = acceptedStats[i].Name;
                //        tRow["Test"] = statNames[j];

                //        hasValue = true;
                //        if (double.IsNaN(accepted) == true) { hasValue = false; }
                //        if (double.IsInfinity(accepted) == true) { hasValue = false; }
                //        if (hasValue == true)
                //        {
                //            tRow["Accepted"] = accepted;
                //        }
                //        AcceptedTable.Rows.Add(tRow);
                //    }
                //}

                //current table
                //Table = AcceptedTable.Copy();

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
                    DataColumn[] currentKeys = new DataColumn[2];
                    currentKeys[0] = currentTable.Columns["Variable"];
                    currentKeys[1] = currentTable.Columns["Test"];
                    currentTable.PrimaryKey = currentKeys;

                    DataColumn[] acceptedKeys = new DataColumn[2];
                    acceptedKeys[0] = acceptedStats.Columns["Variable"];
                    acceptedKeys[1] = acceptedStats.Columns["Test"];
                    acceptedStats.PrimaryKey = acceptedKeys;

                    currentTable.Merge(acceptedStats);
                }
                else
                {
                    currentTable.Columns.Add("Accepted", typeof(double));
                    currentTable.Columns.Add("AcceptedPredictedObservedTestsID", typeof(int));
                }


                //Now add the comparison columns and determine values
                currentTable.Columns.Add("Difference", typeof(double));
                currentTable.Columns.Add("PassedTest", typeof(string));

                foreach (DataRow row in currentTable.Rows)
                {
                    //DataRow[] rowAccepted = AcceptedTable.Select("Name = '" + row["Name"] + "' AND Variable = '" + row["Variable"] + "' AND Test = '" + row["Test"] + "'");
                    //DataRow[] rowCurrent = CurrentTable.Select("Name = '" + row["Name"] + "' AND Variable = '" + row["Variable"] + "' AND Test = '" + row["Test"] + "'");

                    //if (rowAccepted.Count() == 0)
                    //    row["Accepted"] = DBNull.Value;
                    //else
                    //    row["Accepted"] = rowAccepted[0]["Accepted"];

                    //if (rowCurrent.Count() == 0)
                    //    row["Current"] = DBNull.Value;
                    //else
                    //    row["Current"] = rowCurrent[0]["Current"];

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
                    else
                    {
                        row["Difference"] = DBNull.Value;
                        row["PassedTest"] = sigIdent;
                    }
                }

                //Tables could be large so free the memory.
                //AcceptedTable = null;
                //CurrentTable = null;
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