using System;
using System.Web.UI;

namespace APSIM.PerformanceTests.Portal
{
    public partial class ChartsHelp : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblNSEThreshold.Text = TestsCharts.NSEThreshold.ToString();
            lblRSRThreshold.Text = TestsCharts.RSRThreshold.ToString();
        }
    }
}