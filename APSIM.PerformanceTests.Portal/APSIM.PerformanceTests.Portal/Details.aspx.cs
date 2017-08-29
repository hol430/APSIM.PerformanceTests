using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using APSIM.PerformanceTests.Portal.Models;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;

namespace APSIM.PerformanceTests.Portal
{
    public partial class Details : System.Web.UI.Page
    {
        #region Constants and variables

        int predictedObservedId;

        List<vVariable> VariableList;

        #endregion

        #region Page and Control Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["PO_Id"]))
                {
                    predictedObservedId = int.Parse(Request.QueryString["PO_Id"]);
                }

                if (predictedObservedId > 0)
                {
                    hfPredictedObservedID.Value = predictedObservedId.ToString();
                    PredictedObservedDetail currPODetails = PredictedObservedDS.GetByPredictedObservedID(predictedObservedId);
                    ApsimFile apsim = ApsimFilesDS.GetByID(currPODetails.ApsimFilesID);
                    hfPullRequestID.Value = apsim.PullRequestId.ToString();

                    BindCurrentAcceptedTests(currPODetails.ID);

                    lblPullRequest.Text = "Pull Request: " + apsim.PullRequestId.ToString();
                    lblApsimFile.Text = "Apsim File: " + apsim.FileName;
                    lblPOTableName.Text = "Table: " + currPODetails.TableName;

                    BindPredictedObservedVariables(currPODetails.ID);

                    if (gvPOTests.Rows.Count > 0)
                    {
                        //NOTE:  This is registered using the ClientScript (not ScriptManager), with different parameters, as this grid is NOT in an update panel
                        ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_POTests', 'ContentPlaceHolder1_gvPOTests', 'GridHeaderDiv_POTests');</script>");
                    }
                }
            }
            if (gvPOValues.Rows.Count > 0)
            {
                //NOTE:  This is registered using the ScriptManager (not ClientScript), with different parameters, as this grid is nested in an update panel
                ScriptManager.RegisterStartupScript(this, GetType(), "CreateGridHeader_POValues", "CreateGridHeader('GridDataDiv_POValues', 'ContentPlaceHolder1_gvPOValues', 'GridHeaderDiv_POValues');", true);
            }
        }


        /// <summary>
        /// return to the first screen/page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBack_Click(object sender, EventArgs e)
        {
            string pullrequestId = hfPullRequestID.Value.ToString();
            Response.Redirect(string.Format("Default.aspx?PULLREQUEST={0}", pullrequestId));
        }

        /// <summary>
        /// Update the Predicted Observed values graph and grid when the variable changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                lblError.Visible = false;
                //What Variable/Value Name are we working with
                string variable = ddlVariables.SelectedItem.Text;

                //retrieve our predicted observed value
                predictedObservedId = int.Parse(Convert.ToString(hfPredictedObservedID.Value));
                PredictedObservedDetail currPODetails = PredictedObservedDS.GetByPredictedObservedID(predictedObservedId);
                BindCurrentAcceptedValues(variable, currPODetails);
                //BindCurrentAcceptedTests(variable, predictedObservedId);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message.ToString();
                lblError.Visible = true;
            }
        }
        /// <summary>
        /// Set the colour of the rows based on the PassedTests value (true/false)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvPOTests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //this is the true/false column
                if (e.Row.Cells[5].Text.Trim().ToLower() == "false")
                {
                    e.Row.ForeColor = Color.Red;
                }
            }
        }

        /// <summary>
        /// Update the cell background colour of the Predicted Observed data, so that the 'Accepted'
        /// and 'Current' data is highlighted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvPOValues_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //this refers to the "DifferenceDifference" column
                //if (double.Parse(e.Row.Cells[7].Text.Trim()) != 0)
                //{
                //    e.Row.ForeColor = Color.Red;
                //}
                e.Row.Cells[1].BackColor = Color.LightCyan;
                e.Row.Cells[2].BackColor = Color.LightCyan;

                e.Row.Cells[3].BackColor = Color.Gainsboro;
                e.Row.Cells[4].BackColor = Color.Gainsboro;

                if (double.Parse(e.Row.Cells[1].Text.Trim()) != double.Parse(e.Row.Cells[3].Text.Trim()))
                {
                    e.Row.ForeColor = Color.Red;
                }
                if (double.Parse(e.Row.Cells[2].Text.Trim()) != double.Parse(e.Row.Cells[4].Text.Trim()))
                {
                    e.Row.ForeColor = Color.Red;
                }
            }
        }

        /// <summary>
        /// This is used to ensure that the variable data (for the graph and grid) is load after the page_load event. (Lazy Loading)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void LoadTimer_Tick(object sender, EventArgs e)
        {
            //perhaps load the first item
            if (ddlVariables.Items.Count > 0)
            {
                predictedObservedId = int.Parse(Convert.ToString(hfPredictedObservedID.Value));
                string variable = ddlVariables.Items[0].Text;
                BindCurrentAcceptedValues(variable, predictedObservedId);
                LoadTimer.Enabled = false;
            }
        }


        #endregion

        #region Data Retreval and Binding
        /// <summary>
        /// Get the variables that are being reported on for this predicted observed data, and display them in the dropdown
        /// </summary>
        /// <param name="predictedObservedId"></param>
        private void BindPredictedObservedVariables(int predictedObservedId)
        {
            VariableList = PredictedObservedDS.GetVariablesByPredictedObservedID(predictedObservedId);
            ddlVariables.DataSource = VariableList;
            ddlVariables.DataBind();
        }

        /// <summary>
        /// Get the PredictedObserved Details based on the Predicted Observed Id, and then call
        /// BindCurrentAcceptedValues, passing the requrid details
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="predictedObservedId"></param>
        private void BindCurrentAcceptedValues(string variable, int predictedObservedId)
        {
            PredictedObservedDetail currPODetails = PredictedObservedDS.GetByPredictedObservedID(predictedObservedId);
            this.BindCurrentAcceptedValues(variable, currPODetails);
        }

        /// <summary>
        /// Retrieve all of the Predicted Observed values (including simulation details), for the specified variable name, 
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="currPODetails"></param>
        private void BindCurrentAcceptedValues(string variable, PredictedObservedDetail currPODetails)
        {
            lblValues.Text = "Current and Accepted values for " + variable;

            //some of the older records may not have this yet.  So find it the old way.
            if ((currPODetails.AcceptedPredictedObservedDetailsID == null) || (currPODetails.AcceptedPredictedObservedDetailsID <= 0))
            {
                //Retrieve the curresponding (parent) ApsimFile for this PredictedObservedDetail
                ApsimFile apsimFile = ApsimFilesDS.GetByID(currPODetails.ApsimFilesID);

                //get the Pull Request Id for the lastest released pull request that is not the current one
                int acceptPullRequestId = ApsimFilesDS.GetLatestMergedPullRequestId(apsimFile.PullRequestId);

                //get the PredictedObservedDetail.ID for the records that match our current record 'matching' criteria
                currPODetails.AcceptedPredictedObservedDetailsID = PredictedObservedDS.GetIDByMatchingDetails(acceptPullRequestId, apsimFile.FileName, currPODetails.TableName,
                    currPODetails.PredictedTableName, currPODetails.ObservedTableName, currPODetails.FieldNameUsedForMatch);
            }
            //get the data to be displayed in the chart and grid
            List<vCurrentAndAccepted> currValauesData = PredictedObservedDS.GetCurrentAcceptedValues(variable, currPODetails.ID, (int)currPODetails.AcceptedPredictedObservedDetailsID);

            chartPODetails.Titles["chartTitle"].Text = "Current vs Accepted " + variable;

            //Now display it in the chart
            Series acceptedSeries = chartPODetails.Series["Accepted"];
            acceptedSeries.Points.DataBind(currValauesData, "AcceptedObservedValue", "AcceptedPredictedValue", "");

            Series currentSeries = chartPODetails.Series["Current"];
            currentSeries.Points.DataBind(currValauesData, "CurrentObservedValue", "CurrentPredictedValue", "");

            double maxValue = 0;
            double value = 0;
            var maxObject = currValauesData.OrderByDescending(item => item.AcceptedObservedValue).First();
            double.TryParse(maxObject.AcceptedObservedValue.ToString(), out value);
            if (value > maxValue) maxValue = value;

            maxObject = currValauesData.OrderByDescending(item => item.AcceptedPredictedValue).First();
            double.TryParse(maxObject.AcceptedPredictedValue.ToString(), out value);
            if (value > maxValue) maxValue = value;

            maxObject = currValauesData.OrderByDescending(item => item.CurrentObservedValue).First();
            double.TryParse(maxObject.CurrentObservedValue.ToString(), out value);
            if (value > maxValue) maxValue = value;

            maxObject = currValauesData.OrderByDescending(item => item.CurrentPredictedValue).First();
            double.TryParse(maxObject.CurrentPredictedValue.ToString(), out value);
            if (value > maxValue) maxValue = value;

            Series slope = chartPODetails.Series["Slope"];
            slope.Points.AddXY(0, 0);
            slope.Points.AddXY(maxValue, maxValue);

            //And display the combined data and display it in the grid
            gvPOValues.DataSource = currValauesData;
            gvPOValues.DataBind();
            //UpdatePanel2.Update();

            if (gvPOValues.Rows.Count > 0)
            {
                //NOTE:  This is registered using the ScriptManager (not ClientScript), with different parameters, as this grid is nested in an update panel
                ScriptManager.RegisterStartupScript(this, GetType(), "CreateGridHeader_POValues", "CreateGridHeader('GridDataDiv_POValues', 'ContentPlaceHolder1_gvPOValues', 'GridHeaderDiv_POValues');", true);
            }
        }

        /// <summary>
        /// Retrieve the Tests data for the specified predicted observed id
        /// </summary>
        /// <param name="predictedObservedId"></param>
        private void BindCurrentAcceptedTests(int predictedObservedId)
        {

            //lblTests.Text = "Tests for " + variable;
            //get the data to be displayed in the chart and grid
            //List <PredictedObservedTest> currTestsData = PredictedObservedDS.GetCurrentAcceptedTests(predictedObservedId);
            List<PredictedObservedTest> currTestsData = PredictedObservedDS.GetCurrentAcceptedTestsSubset(predictedObservedId);

            //And display the combined data and display it in the grid
            gvPOTests.DataSource = currTestsData;
            gvPOTests.DataBind();

            if (gvPOTests.Rows.Count > 0)
            {
                //NOTE:  This is registered using the ClientScript (not ScriptManager), with different parameters, as this grid is NOT in an update panel
                ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_POTests', 'ContentPlaceHolder1_gvPOTests', 'GridHeaderDiv_POTests');</script>");
            }
        }
        #endregion

    }
}