using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using APSIM.PerformanceTests.Portal.Models;
using System.Drawing;

namespace APSIM.PerformanceTests.Portal
{
    public partial class Default : System.Web.UI.Page
    {
        #region Constants and variables
        private List<vApsimFile> ApsimFileList;
        #endregion

        #region Page and Control Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindApsimFilesGrid();
            }

            if (gvSimFiles.Rows.Count > 0)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv', 'ContentPlaceHolder1_gvSimFiles', 'GridHeaderDiv');</script>");
            }
        }

        protected void gvApsimFiles_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvApsimFiles.PageIndex = e.NewPageIndex;
            BindApsimFilesGrid();
        }

        protected void gvApsimFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvApsimFiles, "Select$" + e.Row.RowIndex);
                e.Row.Attributes["style"] = "cursor:pointer";
            }
        }

        protected void gvApsimFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = gvApsimFiles.SelectedIndex;
            int pullRequestId = int.Parse(gvApsimFiles.Rows[index].Cells[0].Text);
            DateTime runDate = DateTime.Parse(gvApsimFiles.Rows[index].Cells[1].Text);

            lblPullRequestId.Text = string.Format("Simulation Files for Pull Request Id: {0}, on {1}.", pullRequestId.ToString(), runDate.ToString());
            BindSimFilesGrid(pullRequestId, runDate);
        }



        protected void gvSimFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells[3].Text.Equals("100"))
                {
                    e.Row.ForeColor = Color.Green;
                }
                //if (e.Row.Cells[3].Text.Equals("0"))
                //{
                //    e.Row.ForeColor = Color.OrangeRed;
                //}

                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvSimFiles, "Select$" + e.Row.RowIndex);
                e.Row.Attributes["style"] = "cursor:pointer";
            }
        }

        protected void gvSimFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = gvSimFiles.SelectedIndex;
            int predictedObservedtId = int.Parse(gvSimFiles.Rows[index].Cells[0].Text);

            //lblPredictedObserved.Text = "Predicted Observed ID: " + predictedObservedtId.ToString();
            Response.Redirect("Details.aspx?PO_Id=" + predictedObservedtId);
        }

        #endregion


        #region Data Retreval and Binding

        private void BindApsimFilesGrid()
        {
            ApsimFileList = ApsimFilesDS.GetAllApsimFiles();
            gvApsimFiles.DataSource = ApsimFileList;
            gvApsimFiles.DataBind();
        }

        private void BindSimFilesGrid(int pullRequestId, DateTime runDate)
        {
            List<vSimFile> simFiles = ApsimFilesDS.GetSimFilesByPullRequestIDandDate(pullRequestId, runDate);
            gvSimFiles.DataSource = simFiles;
            gvSimFiles.DataBind();

            ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv', 'ContentPlaceHolder1_gvSimFiles', 'GridHeaderDiv');</script>");
        }

        #endregion
    }
}