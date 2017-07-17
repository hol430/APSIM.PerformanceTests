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
                    hfPullRequestID.Value = predictedObservedId.ToString();
                    lblPredictedObserved.Text = "Predicted Observed Details for ID: " + predictedObservedId;
                    BindPredictedObservedVariables();

                    //perhaps load the first item
                    if (ddlVariables.Items.Count > 0)
                    {
                        string variable = ddlVariables.Items[0].Text;
                        BindCurrentAcceptedValues(variable, predictedObservedId);
                    }
                }
                if (gvPODetails.Rows.Count > 0)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_POValues', 'ContentPlaceHolder1_gvPODetails', 'GridHeaderDiv');</script>");
                }

            }
        }

        protected void ddlVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                lblError.Visible = false;
                //What Variable/Value Name are we working with
                string variable = ddlVariables.SelectedItem.Text;

                //retrieve our predicted observed value
                predictedObservedId = int.Parse(Convert.ToString(hfPullRequestID.Value));
                BindCurrentAcceptedValues(variable, predictedObservedId);

            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message.ToString();
                lblError.Visible = true;
            }
        }
        protected void gvPODetails_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (int.Parse(e.Row.Cells[8].Text) != 0)
                {
                    e.Row.ForeColor = Color.Red;
                }
            }
        }

        #endregion

        #region Data Retreval and Binding
        private void BindPredictedObservedVariables()
        {
            VariableList = PredictedObservedDS.GetVariablesByPredictedObservedID(predictedObservedId);
            ddlVariables.DataSource = VariableList;
            ddlVariables.DataBind();
        }

        private void BindCurrentAcceptedValues(string variable, int predictedObservedId)
        {
            //Retrieve the Current PredictedObservedDetail record
            PredictedObservedDetail currPODetails = PredictedObservedDS.GetByPredictedObservedID(predictedObservedId);

            //Retrieve the curresponding (parent) ApsimFile for this PredictedObservedDetail
            ApsimFile apsimFile = ApsimFilesDS.GetByID(currPODetails.ApsimFilesID);

            //get the Pull Request Id for the lastest released pull request that is not the current one
            int acceptPullRequestId = ApsimFilesDS.GetLatestReleasedPullRequestId(apsimFile.PullRequestId);

            //get the PredictedObservedDetail.ID for the records that match our current record 'matching' criteria
            int acceptedPredictedObservedID = PredictedObservedDS.GetIDByMatchingDetails(acceptPullRequestId, apsimFile.FileName, currPODetails.TableName,
                currPODetails.PredictedTableName, currPODetails.ObservedTableName, currPODetails.FieldNameUsedForMatch);

            //get the data to be displayed in the chart and grid
            List<vCurrentAndAccepted> currValauesData = PredictedObservedDS.GetCurrentAcceptedValues(variable, currPODetails.ID, acceptedPredictedObservedID);

            chartPODetails.Titles["chartTitle"].Text = "Current vs Accepted " + variable;

            //Now display it in the chart
            Series acceptedSeries = chartPODetails.Series["Accepted"];
            acceptedSeries.Points.DataBind(currValauesData, "AcceptedObservedValue", "AcceptedPredictedValue", "");

            Series currentSeries = chartPODetails.Series["Current"];
            currentSeries.Points.DataBind(currValauesData, "CurrentObservedValue", "CurrentPredictedValue", "");

            //And display the combined data and display it in the grid
            gvPODetails.DataSource = currValauesData;
            gvPODetails.DataBind();

            if (gvPODetails.Rows.Count > 0)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_POValues', 'ContentPlaceHolder1_gvPODetails', 'GridHeaderDiv');</script>");
            }
        }

        #endregion
    }
}