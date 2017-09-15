using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using APSIM.PerformanceTests.Portal.Models;


namespace APSIM.PerformanceTests.Portal
{
    public partial class Compare : System.Web.UI.Page
    {
        #region Constants and variables
        
        #endregion

        #region Page and Control Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindPullRequestLists();
            }
        }


        protected void btnCompare_Click(object sender, EventArgs e)
        {

        }


        protected void ddlVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //lblError.Visible = false;
                ////What Variable/Value Name are we working with
                //string variable = ddlVariables.SelectedItem.Text;

                ////retrieve our predicted observed value
                //predictedObservedId = int.Parse(Convert.ToString(hfPredictedObservedID.Value));
                //PredictedObservedDetail currPODetails = PredictedObservedDS.GetByPredictedObservedID(predictedObservedId);
                //BindCurrentAcceptedValues(variable, currPODetails);
                ////BindCurrentAcceptedTests(variable, predictedObservedId);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message.ToString();
                lblError.Visible = true;
            }
        }

        protected void gvApsimFiles1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            e.Row.Attributes["onmouseover"] = "this.style.cursor='hand';" + "this.originalBackgroundColor=this.style.backgroundColor;" + "this.style.backgroundColor='#bbbbbb';";
            e.Row.Attributes["onmouseout"] = "this.style.backgroundColor=this.originalBackgroundColor;";
            e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(this.gvApsimFiles1, "Select$" + e.Row.RowIndex);
        }
        protected void gvApsimFiles1_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtPullRequest1.Text = "";
            if (gvApsimFiles1.SelectedRow != null)
            {
                int pullRequestId = int.Parse(Server.HtmlDecode(gvApsimFiles1.SelectedRow.Cells[0].Text));
                txtPullRequest1.Text = pullRequestId.ToString() + " - " + Server.HtmlDecode(gvApsimFiles1.SelectedRow.Cells[1].Text);
                BindSimulationFiles(pullRequestId);
            }
            DetermineVisibility_FileAndFilenamePanel();
        }

        protected void gvApsimFiles2_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            e.Row.Attributes["onmouseover"] = "this.style.cursor='hand';" + "this.originalBackgroundColor=this.style.backgroundColor;" + "this.style.backgroundColor='#bbbbbb';";
            e.Row.Attributes["onmouseout"] = "this.style.backgroundColor=this.originalBackgroundColor;";
            e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(this.gvApsimFiles2, "Select$" + e.Row.RowIndex);
        }
        protected void gvApsimFiles2_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtPullRequest2.Text = "";
            if (gvApsimFiles2.SelectedRow != null)
            {
                txtPullRequest2.Text = Server.HtmlDecode(gvApsimFiles2.SelectedRow.Cells[0].Text) + " - " + Server.HtmlDecode(gvApsimFiles2.SelectedRow.Cells[1].Text);
            }
            DetermineVisibility_FileAndFilenamePanel();
        }

        protected void gvSimFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            e.Row.Attributes["onmouseover"] = "this.style.cursor='hand';" + "this.originalBackgroundColor=this.style.backgroundColor;" + "this.style.backgroundColor='#bbbbbb';";
            e.Row.Attributes["onmouseout"] = "this.style.backgroundColor=this.originalBackgroundColor;";
            e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(this.gvSimFiles, "Select$" + e.Row.RowIndex);
        }
        protected void gvSimFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSimFiles.Text = "";
            if (gvSimFiles.SelectedRow != null)
            {
                int predictedObservedId = int.Parse(Server.HtmlDecode(gvSimFiles.SelectedRow.Cells[0].Text));
                txtPredictedObservedID1.Text = predictedObservedId.ToString();
                txtSimFiles.Text = Server.HtmlDecode(gvSimFiles.SelectedRow.Cells[1].Text) + " - " + Server.HtmlDecode(gvSimFiles.SelectedRow.Cells[2].Text);
                //Now get pull request details so that we can find the  predictedObservedId for the 2nd Pull Request ID
                int altPullRequestId = int.Parse(txtPullRequest2.Text.Split('-')[0].Trim());
                txtPredictedObservedID2.Text = GetPredictedObservedIDforPullRequestID(predictedObservedId, altPullRequestId, Server.HtmlDecode(gvSimFiles.SelectedRow.Cells[1].Text), Server.HtmlDecode(gvSimFiles.SelectedRow.Cells[2].Text));
                DetermineVisibility_pnlPredictedObservedIds();
            }
        }

        private void DetermineVisibility_FileAndFilenamePanel()
        {
            if ((txtPullRequest1.Text.Length > 0) && (txtPullRequest2.Text.Length > 0))
            {
                pnlFileAndTableName.Visible = true;
                DetermineVisibility_pnlPredictedObservedIds();
            }
            else
            {
                pnlFileAndTableName.Visible = false;
                pnlPredictedObservedIds.Visible = false;
            }
        }
        private void DetermineVisibility_pnlPredictedObservedIds()
        {
            if ((txtPredictedObservedID1.Text.Length > 0) & (txtPredictedObservedID2.Text.Length > 0))
            {
                pnlPredictedObservedIds.Visible = true;
            }
            else
            {
                pnlPredictedObservedIds.Visible = false;
            }
        }
        #endregion


        #region Data Retreval and Binding

        //this will provide data for both Pull request drop down controls
        private void BindPullRequestLists()
        {
            List<vApsimFile> apsimFileList= ApsimFilesDS.GetPullRequestsWithStatus();

            gvApsimFiles1.DataSource = apsimFileList;
            gvApsimFiles1.DataBind();

            gvApsimFiles2.DataSource = apsimFileList;
            gvApsimFiles2.DataBind();
        }

        private void BindSimulationFiles(int pullRequestId)
        {
            List<vSimFile> simFileList = ApsimFilesDS.GetSimFilesByPullRequestID(pullRequestId);
            gvSimFiles.DataSource = simFileList;
            gvSimFiles.DataBind();

            //Can we 
            //BindPredictedObservedVariables()
        }

        private string GetPredictedObservedIDforPullRequestID(int predictedObservedId, int pullRequestId, string fileName, string tablename)
        {
            int altAcceptedPredictedObservedDetailsID = 0;
            try
            {
                //get the PredictedObservedDetail.ID for the records that match our current record 'matching' criteria
                altAcceptedPredictedObservedDetailsID = PredictedObservedDS.GetIDByMatchingDetails(pullRequestId, fileName, tablename);
            }
            catch (Exception)
            {
            }
            return altAcceptedPredictedObservedDetailsID.ToString();
        }

        private void BindPredictedObservedVariables(int predictedObservedId)
        {
            List<vVariable> VariableList = PredictedObservedDS.GetVariablesByPredictedObservedID(predictedObservedId);
            ddlVariables.DataSource = VariableList;
            ddlVariables.DataBind();
        }

        #endregion
    }
}