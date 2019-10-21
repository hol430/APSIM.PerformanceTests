using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.DataVisualization.Charting;
using APSIM.PerformanceTests.Portal.Models;
using System.Drawing.Imaging;
using System.IO;
using System.Web.UI.HtmlControls;

namespace APSIM.PerformanceTests.Portal
{
    public partial class TestsCharts : System.Web.UI.Page
    {
        /// <summary>
        /// 'Worst' value of NSE. NSE will appear black in the heatmap if it is
        /// less than or equal to this. Cannot be 1.
        /// </summary>
        public const double NSEThreshold = 0;

        /// <summary>
        /// 'Worst' value of RSR. RSR will appear black in the heatmap if it is
        /// greater than or equal to this.
        /// </summary>
        public const double RSRThreshold = 1;

        #region Page and Control Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["PULLREQUEST"] != null)
            {
                int pullRequestId = int.Parse(Request.QueryString["PULLREQUEST"].ToString());
                hfPullRequestID.Value = pullRequestId.ToString();
                lblPullRequest.Text = "Pull Request Id: " + pullRequestId.ToString();
                RetrieveDataAndBindCharts();
            }
            else
            {
                hfPullRequestID.Value = "3551";
                lblPullRequest.Text = "Pull Request Id: 3551";
                RetrieveDataAndBindCharts();
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            string pullrequestId = hfPullRequestID.Value.ToString();
            Response.Redirect(string.Format("Default.aspx?PULLREQUEST={0}", pullrequestId));
        }


        private void myChart_Click(object sender, System.Web.UI.WebControls.ImageMapEventArgs e)
        {
            string PO_Id = e.PostBackValue;
            Response.Redirect(string.Format("PODetails.aspx?PO_Id={0}", PO_Id));
        }

        #endregion

        #region Data Retreval and Binding


        private void RetrieveDataAndBindCharts()
        {
            int pullRequestId = int.Parse(hfPullRequestID.Value.ToString());
            List<vPredictedObservedTests> POTestsList = PredictedObservedDS.GetCurrentAcceptedTests(pullRequestId);

            bool newchart = false;
            string holdFileName = string.Empty;
            string holdTableName = string.Empty;
            string holdVariable = string.Empty;
            string holdTitle = string.Empty;
            string holdPO_Id = string.Empty;
            string tooltip = string.Empty;

            GenerateHeatmap(POTestsList);

            List<string> AcceptedXValues = new List<string>();
            List<double> AcceptedYValues = new List<double>(); 
            List<Color> AcceptedColours = new List<Color>();
            List<string> CurrentXValues = new List<string>();
            List<double> CurrentYValues = new List<double>();
            List<Color> CurrentColours = new List<Color>();


            Color currColour, accColour;
            int chartNo = 0;

            foreach (vPredictedObservedTests item in POTestsList)
            {

                //this is the first instance
                if (holdFileName == string.Empty)
                {
                    holdFileName = item.FileName;
                    UpdatePlaceHolderWithTitle(phCharts, item.FileName, false);

                    holdTableName = item.TableName;
                    holdVariable = item.Variable;

                    holdPO_Id = item.PredictedObservedDetailsID.ToString();
                }

                newchart = false;
                if (item.FileName != holdFileName) { newchart = true; }
                if (item.TableName != holdTableName) { newchart = true; }
                if (item.Variable != holdVariable) { newchart = true; }

                if (newchart == true)
                {
                    if ((AcceptedXValues.Count > 0) || (CurrentXValues.Count > 0))
                    {
                        chartNo += 1;
                        tooltip = string.Format("{0} - {1}", holdFileName, holdTableName);
                        CreateCharts(chartNo, holdVariable, holdPO_Id, tooltip, AcceptedColours.ToArray(), AcceptedXValues.ToArray(), AcceptedYValues.ToArray(),
                            CurrentColours.ToArray(), CurrentXValues.ToArray(), CurrentYValues.ToArray());
                    }

                    if (item.FileName != holdFileName)
                    {
                        UpdatePlaceHolderWithTitle(phCharts, item.FileName, true);
                    }

                    //clear these for next time
                    AcceptedXValues = new List<string>();
                    AcceptedYValues = new List<double>();
                    AcceptedColours = new List<Color>();
                    CurrentXValues = new List<string>();
                    CurrentYValues = new List<double>();
                    CurrentColours = new List<Color>();
                    holdFileName = item.FileName;
                    holdTableName = item.TableName;
                    holdVariable = item.Variable;
                    holdPO_Id = item.PredictedObservedDetailsID.ToString();
                }

                if ((item.Test != "n") && (item.Test != "RMSE"))
                {

                    if (item.Accepted != null)
                    {
                        AcceptedXValues.Add(item.Test);
                        AcceptedYValues.Add((double)item.Accepted);
                        if (Math.Abs((double)item.Accepted) > 1)
                        {
                            accColour = Color.Black;
                        }
                        else
                        {
                            accColour = Color.Gray;
                        }
                        AcceptedColours.Add(accColour);
                    }

                    if (item.Current != null)
                    {
                        try
                        {
                            CurrentXValues.Add(item.Test);
                            CurrentYValues.Add((double)item.Current);
                            if (Math.Abs((double)item.Current) > 1)
                            {
                                currColour = Color.Orange;
                            }
                            else if ((bool)item.IsImprovement)
                            {
                                currColour = Color.Green;
                            }
                            else if ((bool)item.PassedTest)
                            {
                                currColour = Color.White;
                            }
                            else if (item.Accepted != null) 
                            {
                                if ((double)item.Current == (double)item.Accepted)
                                {
                                    currColour = Color.Gray;
                                }
                                else
                                {
                                    currColour = Color.Red;
                                }
                            }
                            else
                            {
                                currColour = Color.Red;
                            }
                            CurrentColours.Add(currColour);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            if ((AcceptedXValues.Count > 0) || (CurrentXValues.Count > 0))
            {
                chartNo += 1;
                tooltip = string.Format("{0} - {1}", holdFileName, holdTableName);
                CreateCharts(chartNo, holdVariable, holdPO_Id, tooltip, AcceptedColours.ToArray(), AcceptedXValues.ToArray(), AcceptedYValues.ToArray(),
                    CurrentColours.ToArray(), CurrentXValues.ToArray(), CurrentYValues.ToArray());
            }
        }

        /// <summary>
        /// Generates a table of heatmaps of the data.
        /// Each row represents stats for one model (wheat, barley, sorghum, etc.).
        /// Each column represents one particular statistic (r2, nse, etc.).
        /// </summary>
        /// <param name="poTestsList"></param>
        private void GenerateHeatmap(List<vPredictedObservedTests> poTestsList)
        {
            Table heatmapTable = new Table();
            TableRow row;
            TableCell cell;
            ImageButton dataPoint; // This represents one data point (pixel) in a heatmap.
            HtmlGenericControl div; // This wraps the heatmap inside each cell.

            // First, create a row of column headers, containing the test names.
            // For now, we will not graph RMSE, as it is in the units of the variable it describes.
            string[] testNames = poTestsList.Select(po => po.Test).Distinct().Where(t => !t.Equals("RMSE", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            row = new TableRow();

            // Left-most column contains model names.
            cell = new TableHeaderCell();
            cell.Text = "Model Name";
            row.Cells.Add(cell);

            // The remaining cells in the top row contain the test names (nse, r2, etc).
            foreach (string testName in testNames)
            {
                cell = new TableHeaderCell();
                cell.Text = testName;
                row.Cells.Add(cell);
            }
            heatmapTable.Rows.Add(row);

            // Next, iterate over each model in the data. These will be our rows.
            foreach (var model in poTestsList.GroupBy(v => v.FileName))
            {
                row = new TableRow();

                // The first cell in each row will contain the model name.
                cell = new TableHeaderCell();
                cell.Text = model.Key;
                row.Cells.Add(cell);

                foreach (var test in model.GroupBy(v => v.Test))
                {
                    // We don't want to generate a heatmap for every test.
                    if (!testNames.Contains(test.Key))
                        continue;

                    // Each cell contains a heatmap of data. We display this in
                    // a square rather than a line, to conserve space.
                    cell = new TableCell();

                    // Each heatmap goes inside a div, which goes inside a cell.
                    // This means we can put a border around the heatmap, not the
                    // cell, which can be bigger than the heatmap. Without the border,
                    // it can be very difficult to see where the heatmaps start/end.
                    div = new HtmlGenericControl("div");
                    div.Style.Add("border", "1px solid black");
                    div.Style.Add("display", "inline-block");
                    div.Style.Add("overflow", "hidden");

                    // Area of the heatmap will be the smallest square number which
                    // is larger than the number of data points in the heatmap.
                    // The length of each row will be the square root of this number.
                    int rowLength = (int)Math.Floor(Math.Sqrt(test.Count())) + 1;

                    // Our data contains many nullable doubles (ugh) so let's filter
                    // them out before we start iteration, otherwise it will mess up
                    // our indexing.
                    List<vPredictedObservedTests> testWithoutNulls = test.Where(v => v.Current != null && v.Accepted != null).ToList();

                    for (int i = 0; i < testWithoutNulls.Count; i++)
                    {
                        vPredictedObservedTests item = testWithoutNulls[i];
                        dataPoint = new ImageButton();
                        dataPoint.ImageUrl = GetImageUrl(item);

                        // Embed PO ID in the image.
                        dataPoint.Attributes["POID"] = item.PredictedObservedDetailsID.ToString();

                        // The last item in each row needs to be "display: block;"
                        // All other items need to be "float: left;"
                        if ( ((i + 1) % rowLength) == 0)
                            dataPoint.Style.Add("display", "block");
                        else
                            dataPoint.Style.Add("float", "left");

                        dataPoint.ToolTip = item.Variable + " " + item.Test;
                        dataPoint.Click += OnHeatmapPixelClicked;
                        div.Controls.Add(dataPoint);
                    }
                    cell.Controls.Add(div);
                    row.Cells.Add(cell);
                }
                heatmapTable.Rows.Add(row);
            }
            phHeatmap.Controls.Add(heatmapTable);
        }

        /// <summary>
        /// Gets a URL for a coloured square to be used in the heatmap for a 
        /// given PredictedObserved test.
        /// </summary>
        /// <param name="item">The item for which we want a colour.</param>
        private static string GetImageUrl(vPredictedObservedTests item)
        {
            Color colour = GetColour(item);
            string url = $"/APSIM.PerformanceTests/WebForm1.aspx?a={colour.A}&r={colour.R}&g={colour.G}&b={colour.B}";
#if DEBUG
            url = $"/WebForm1.aspx?a={colour.A}&r={colour.R}&g={colour.G}&b={colour.B}";
#endif
            return url;
        }

        /// <summary>
        /// Generates a colour for an item, for use in the heatmap.
        /// </summary>
        /// <param name="item">The item for which we need a colour.</param>
        private static Color GetColour(vPredictedObservedTests item)
        {
            double intensity = Math.Abs( ((double)item.Current - (double)item.Accepted) / (double)item.Accepted);
            intensity = Math.Min(intensity, 1); // Upper bound = 1.
            if (item.IsImprovement != null && (bool)item.IsImprovement)
                return GetGreen(intensity);
            else if (item.PassedTest != null && (bool)item.PassedTest)
                return GetGreyscaleColour(GetColourIntensity(item));
            else
                return GetRed(1 - intensity); // Darker red = worse, so invert the intensity.
        }

        /// <summary>
        /// Gets an estimation of the 'goodness' of an item's result, for use
        /// in the heatmap.
        /// </summary>
        /// <param name="item">The item for which we need an intensity.</param>
        /// <returns>A double in the range [0, 1].</returns>
        private static double GetColourIntensity(vPredictedObservedTests item)
        {
            switch (item.Test.ToUpper())
            {
                case "N":
                    return item.Current == item.Accepted ? 1 : 0;
                case "NSE":
                    return NormaliseNse((double)item.Current);
                case "R2":
                    return (double)item.Current;
                case "RSR":
                    return NormaliseNse(1 - (double)item.Current);
                default:
                    throw new Exception($"unknown statistic: {item.Test}");
            }
        }

        /// <summary>
        /// Takes an RSR value in the range [0, inf] and normalises it,
        /// returning a number in the range [0, 1], where 1 represents a
        /// perfect fit, and 0 represents a very bad fit.
        /// </summary>
        /// <param name="value">The RSR value.</param>
        /// <returns>
        /// A value greater than `RSRThreshold` returns 0.
        /// Otherwise, the result scales linearly: f(0) = 1, f(RSRThreshold) = 0.
        /// </returns>
        private double NormaliseRsr(double value)
        {
            if (value < RSRThreshold)
                return 1 - (value / RSRThreshold); // f(0) = 1, f(RSRThreshold) = 0
            else
                return 0;
        }

        /// <summary>
        /// Takes an NSE value in the range [-inf, 1] normalises it, returning
        /// a number in the range [0, 1], where 1 represents a
        /// perfect fit, and 0 represents a very bad fit (NSE < <see cref="NSEThreshold"/>).
        /// </summary>
        /// <param name="value">The NSE value.</param>
        /// <returns></returns>
        /// <remarks>
        /// A value less than `NSEThreshold` will return 0.
        /// Otherwise, the result scales linearly: f(1) = 1, f(NSEThreshold) = 0.
        /// </remarks>
        private static double NormaliseNse(double value)
        {
            if (value > NSEThreshold)
                return (value - 1) / (1 - NSEThreshold) + 1; // f(1) = 1, f(NSEThreshold) = 0
            else
                return 0;
        }

        /// <summary>
        /// Gets a shade of green with a given intensity in the range [0, 1].
        /// Higher number represents more intense shade of green.
        /// A value of 0 will return #008000.
        /// </summary>
        /// <param name="intensity">
        /// Intensity of the shade in the range [0, 1].
        /// Value of 0 represents very dark green (#008000).
        /// Value of 1 represents full intensity (#FF0000).
        /// </param>
        /// <returns>Shade of green.</returns>
        private static Color GetGreen(double intensity)
        {
            if (intensity < 0 || intensity > 1)
                throw new Exception($"value out of range: {intensity}");
            int shade = (int)Math.Floor(intensity * 127) + 128; // min value of 128
            return Color.FromArgb(0, shade, 0);
        }

        /// <summary>
        /// Gets a shade of red with a given intensity in the range [0, 1].
        /// Higher number represents more intense shade of red.
        /// </summary>
        /// <param name="intensity">
        /// Intensity of the shade in the range [0, 1].
        /// Value of 0 represents very dark red (#800000).
        /// Value of 1 represents full intensity (#FF0000).
        /// </param>
        /// <returns>Shade of Red.</returns>
        private static Color GetRed(double intensity)
        {
            if (intensity < 0 || intensity > 1)
                throw new Exception($"value out of range: {intensity}");
            int shade = (int)Math.Floor(intensity * 127) + 128; // min value of 128
            return Color.FromArgb(shade, 0, 0);
        }

        /// <summary>
        /// Gets a greyscale colour from a standardised value in the range [0, 1],
        /// where a value of 1 represents white (perfect) and a value of
        /// 0 represents black (bad).
        /// </summary>
        public static Color GetGreyscaleColour(double value)
        {
            if (value < 0 || value > 1)
                throw new Exception($"value out of range: {value}");
            int shade = (int)Math.Floor(value * 255);
            return Color.FromArgb(shade, shade, shade);
        }

        /// <summary>
        /// Navigates to the ValuesCharts page, which contains more detailed info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Assumes that each heatmap pixel's css class contains the PO_Id.
        /// </remarks>
        private void OnHeatmapPixelClicked(object sender, ImageClickEventArgs e)
        {
            ImageButton image = sender as ImageButton;
            if (image != null)
                Response.Redirect("ValuesCharts.aspx?PO_Id=" + image.Attributes["POID"]);
        }

        private void UpdatePlaceHolderWithTitle(PlaceHolder ph, string name, bool addSpace)
        {
            if (addSpace == true)
            {
                phCharts.Controls.Add(new LiteralControl("<br />"));
            }
            phCharts.Controls.Add(new LiteralControl("<hr>"));
            Label lblFileName = new Label();
            lblFileName.Text = name;
            lblFileName.CssClass = "SectionTitles";
            phCharts.Controls.Add(lblFileName);
            phCharts.Controls.Add(new LiteralControl("<br />"));

        }

        private void CreateCharts(int chartNo, string chartTitle, string PO_id, string tooltip, Color[] acceptedColours, string[] acceptedXValues, double[] acceptedYValues,
            Color[] currentColours, string[] currentXValues, double[] currentYValues)
        {
            Chart myChart = new Chart();
            myChart.Height = 200;

            myChart.Titles.Add(chartTitle);
            myChart.ChartAreas.Add("ChartArea1");
            myChart.ToolTip = tooltip;
            //this needs to be unique for each chart for the click event to work
            myChart.ID = chartNo.ToString();


            //Add the accepted Series
            myChart.Series.Add("Accepted");
            myChart.Series["Accepted"].ChartArea = "ChartArea1";
            myChart.Series["Accepted"].ChartType = SeriesChartType.Column;
            //myChart.Series["Accepted"].Url = string.Format("Details.aspx?PO_Id={0}", PO_id);
            myChart.Series["Accepted"].Url = string.Format("ValuesCharts.aspx?PO_Id={0}", PO_id);
            myChart.Series["Accepted"].Points.DataBindXY(acceptedXValues, acceptedYValues);
            for (int i = 0; i < acceptedColours.Count(); i++)
            {
                myChart.Series["Accepted"].Points[i].Color = acceptedColours[i];
            }
            //if (myChart.Series["Accepted"].Points.Count > 0)
            //{
            //    for (int i = 0; i < myChart.Series["Accepted"].Points.Count; i++)
            //    {
            //        DataPoint myDataPoint = myChart.Series["Accepted"].Points[i];
            //        myDataPoint.Url = string.Format("Details.aspx?PO_Id={0}", PO_id);
            //    }
            //}


            //Now add the Current Series
            myChart.Series.Add("Current");
            myChart.Series["Current"].ChartArea = "ChartArea1";
            myChart.Series["Current"].ChartType = SeriesChartType.Column;
            //myChart.Series["Current"].Url = string.Format("Details.aspx?PO_Id={0}", PO_id);
            myChart.Series["Current"].Url = string.Format("ValuesCharts.aspx?PO_Id={0}", PO_id);

            myChart.Series["Current"].Points.DataBindXY(currentXValues, currentYValues);
            for (int i = 0; i < currentColours.Count(); i++)
            {
                myChart.Series["Current"].Points[i].BorderColor = Color.Black;
                myChart.Series["Current"].Points[i].Color = currentColours[i];
            }

            phCharts.Controls.Add(myChart);
        }
        #endregion

    }
}