using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APSIM.PerformanceTests.Portal
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblFooter.Text = string.Format("&copy; {0} - APSIM.PerformanceTests.Portal", DateTime.Today.Year.ToString());
        }
    }
}