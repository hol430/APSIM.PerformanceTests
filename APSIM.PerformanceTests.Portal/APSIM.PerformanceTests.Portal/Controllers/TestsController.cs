using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;

using APSIM.PerformanceTests.Portal.ModelViews;
using APSIM.PerformanceTests.Portal.DataAccessLayer;


namespace APSIM.PerformanceTests.Portal.Controllers
{
    public class TestsController : Controller
    {
        private ApsimDBContext db = new ApsimDBContext();

        // GET: Test
        public ActionResult Index(int? predictedObservedId)
        {
            TestsIndex tests = new TestsIndex();
            if (predictedObservedId != null)
            {
                ViewBag.PredictedObservedId = predictedObservedId;

                tests.PO_Id = predictedObservedId;
                tests.POTests = db.PredictedObservedTests
                                    .Where(t => t.PredictedObservedDetailsID == predictedObservedId)
                                    .Select(t => new POTest
                                    {
                                        Variable = t.Variable,
                                        PredictedObservedDetailsID = t.PredictedObservedDetailsID,
                                        Test = t.Test,
                                        Accepted = t.Accepted,
                                        Current = t.Current,
                                        Difference = t.Difference,
                                        PassedTest = t.PassedTest,
                                    })
                                    .Distinct()
                                    .OrderBy(t => t.Variable);

                //get the PO Table Name so it can be displayed
                tests.PO_TableName = db.PredictedObservedDetails
                                    .Where(d => d.ID == predictedObservedId)
                                    .Select(d => d.TableName)
                                    .SingleOrDefault();
            }
            return View(tests);
        }


        public FileResult CreateChart(int? predictedObservedId)
        {

            //this was originally being passed;
            SeriesChartType chartType = SeriesChartType.Point;

            IEnumerable<POTest> tests = db.PredictedObservedTests
                                .Where(t => t.PredictedObservedDetailsID == predictedObservedId)
                                .Select(t => new POTest
                                {
                                    Variable = t.Variable,
                                    PredictedObservedDetailsID = t.PredictedObservedDetailsID,
                                    Test = t.Test,
                                    Accepted = t.Accepted,
                                    Current = t.Current,
                                    Difference = t.Difference,
                                    PassedTest = t.PassedTest,
                                })
                                .Distinct()
                                .OrderBy(t => t.Variable);

            Chart chart = new Chart();
            chart.Width = 800;
            chart.Height = 500;
            //chart.BackColor = Color.FromArgb(211, 223, 240);
            //chart.BorderlineDashStyle = ChartDashStyle.Solid;
            //chart.BackSecondaryColor = Color.White;
            //chart.BackGradientStyle = GradientStyle.TopBottom;
            //chart.BorderlineWidth = 1;
            //chart.Palette = ChartColorPalette.BrightPastel;
            //chart.BorderlineColor = Color.FromArgb(26, 59, 105);
            chart.RenderType = RenderType.BinaryStreaming;
            //chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            //chart.AntiAliasing = AntiAliasingStyles.All;
            //chart.TextAntiAliasingQuality = TextAntiAliasingQuality.Normal;
            chart.Titles.Add(CreateTitle("Predicted Observed Tests Comparisons"));
            chart.Legends.Add(CreateLegend());
            chart.Series.Add(CreateSeries(tests, chartType));
            chart.ChartAreas.Add(CreateChartArea());

            MemoryStream ms = new MemoryStream();
            chart.SaveImage(ms);
            return File(ms.GetBuffer(), @"image/png");
        }

        [NonAction]
        public Title CreateTitle(string chartTitle)
        {
            Title title = new Title();
            title.Text = chartTitle;
            title.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            title.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ShadowOffset = 3;
            title.ForeColor = Color.FromArgb(26, 59, 105);

            return title;
        }

        [NonAction]
        public Legend CreateLegend()
        {
            Legend legend = new Legend();
            legend.Name = "Result Chart";
            legend.Docking = Docking.Bottom;
            legend.Alignment = StringAlignment.Center;
            legend.BackColor = Color.Transparent;
            legend.Font = new Font(new FontFamily("Trebuchet MS"), 9);
            legend.LegendStyle = LegendStyle.Row;

            return legend;
        }

        [NonAction]
        public Series CreateSeries(IEnumerable<POTest> tests, SeriesChartType chartType)
        {
            Series seriesDetail = new Series();
            seriesDetail.Name = "Default";
            seriesDetail.IsValueShownAsLabel = false;
            seriesDetail.Color = Color.FromArgb(198, 99, 99);
            seriesDetail.ChartType = chartType;
            seriesDetail.BorderWidth = 2;
            //seriesDetail["DrawingStyle"] = "Cylinder";
            //seriesDetail["PieDrawingStyle"] = "SoftEdge";

            DataPoint point;
            
            foreach (POTest item in tests)
            {
                point = new DataPoint();
                point.SetValueXY(item.Current, item.Accepted);
                seriesDetail.Points.Add(point);
            }
            seriesDetail.ChartArea = "Result Chart";

            return seriesDetail;
        }

        [NonAction]
        public ChartArea CreateChartArea()
        {
            ChartArea chartArea = new ChartArea();
            chartArea.Name = "Result Chart";
            chartArea.BackColor = Color.Transparent;
            //chartArea.AxisX.IsLabelAutoFit = false;
            //chartArea.AxisY.IsLabelAutoFit = false;
            //chartArea.AxisX.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            //chartArea.AxisY.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            //chartArea.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            //chartArea.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            //chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            //chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            //chartArea.AxisX.Interval = 1;
            // chartArea.Position.Width = 98;
            // chartArea.Position.Height = 70;
            // chartArea.Position.Y = 15;
            // chartArea.Position.X = 0;
            //chartArea.Area3DStyle.Enable3D = true;
            /*chartArea.Area3DStyle.Rotation = 10;
            chartArea.Area3DStyle.Perspective = 10;
            chartArea.Area3DStyle.Inclination = 15;
            chartArea.Area3DStyle.IsRightAngleAxes=false;
            chartArea.Area3DStyle.WallWidth=0;
            chartArea.Area3DStyle.IsClustered=false;*/
            return chartArea;
        }

    }
}