using APSIM.PerformanceTests.Portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Net.Http;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;


namespace APSIM.PerformanceTests.Portal
{
    public partial class Difference : System.Web.UI.Page
    {
        #region Constants and variables
        private List<vPredictedObservedTests> POTestsList;
        private DataTable POTestsDT;

        public string SortDireaction_POTestsList
        {
            get
            {
                if (ViewState["SortDireaction_POTestsList"] == null)
                    return string.Empty;
                else
                    return ViewState["SortDireaction_POTestsList"].ToString();
            }
            set
            {
                ViewState["SortDireaction_POTestsList"] = value;
            }
        }
        private string _sortDirection_POTestsList;

        #endregion


        #region Page and Control Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Request.QueryString["PULLREQUEST"] != null)
                {
                    int pullRequestId = int.Parse(Request.QueryString["PULLREQUEST"].ToString());
                    hfPullRequestID.Value = pullRequestId.ToString();
                    lblPullRequest.Text = "Pull Request Id: " + pullRequestId.ToString();
                    Session["POTestsDT"] = null;
                    BindPredictedObservedTestsDiffsDataTable();
                }
            }
        }


        protected void btnBack_Click(object sender, EventArgs e)
        {
            string pullrequestId = hfPullRequestID.Value.ToString();
            Response.Redirect(string.Format("Default.aspx?PULLREQUEST={0}", pullrequestId));
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterPredictedObservedTestsDiffs(txtSearch.Text);
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvPOTests.PageIndex = 0;
            BindPredictedObservedTestsDiffsDataTable();
        }


        protected void gvPOTests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPOTests.PageIndex = e.NewPageIndex;
            BindPredictedObservedTestsDiffsDataTable();
        }

        protected void gvPOTests_RowCommand(object sender, GridViewCommandEventArgs e)
        {

        }

        protected void gvPOTests_Sorting(object sender, GridViewSortEventArgs e)
        {
            SetSortDirection("gvPOTests", SortDireaction_POTestsList);
            BindPredictedObservedTestsDiffsDataTable();

            if (POTestsDT != null)
            {
                if (e.SortExpression == "DifferencePercent")
                {
                    POTestsDT.DefaultView.Sort = e.SortExpression + " " + _sortDirection_POTestsList;
                }
                else if (e.SortExpression == "PassedTest")
                {
                    POTestsDT.DefaultView.Sort = e.SortExpression + " " + _sortDirection_POTestsList + ", DifferencePercent DESC";
                }
                else
                {
                    POTestsDT.DefaultView.Sort = e.SortExpression + " " + _sortDirection_POTestsList + ", PassedTest, DifferencePercent DESC";
                }

                //Sort the data.
                gvPOTests.DataSource = POTestsDT;
                gvPOTests.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
                gvPOTests.DataBind();
                SortDireaction_POTestsList = _sortDirection_POTestsList;

                int sortColumnIndex = 0;
                foreach (DataControlFieldHeaderCell headerCell in gvPOTests.HeaderRow.Cells)
                {
                    //Make sure we are displaying the correct header for all columns
                    switch (headerCell.ContainingField.SortExpression)
                    {
                        case "FileName":
                            gvPOTests.Columns[0].HeaderText = "Apsim<br />File Name";
                            break;
                        case "TableName":
                            gvPOTests.Columns[1].HeaderText = "PredictedObserved<br />TableName";
                            break;
                        case "Variable":
                            gvPOTests.Columns[2].HeaderText = "Variable";
                            break;
                        case "Test":
                            gvPOTests.Columns[3].HeaderText = "Test";
                            break;
                        case "Accepted":
                            gvPOTests.Columns[4].HeaderText = "Accepted";
                            break;
                        case "Current":
                            gvPOTests.Columns[5].HeaderText = "Current";
                            break;
                        case "Difference":
                            gvPOTests.Columns[6].HeaderText = "Difference";
                            break;
                        case "DifferencePercent":
                            gvPOTests.Columns[7].HeaderText = "Difference<br />Percent";
                            break;
                        case "PassedTest":
                            gvPOTests.Columns[8].HeaderText = "Passed<br />Test";
                            break;
                        case "IsImprovement":
                            gvPOTests.Columns[9].HeaderText = "Is<br />Improvement";
                            break;
                        case "PredictedObservedDetailsID":
                            gvPOTests.Columns[10].HeaderText = "PO Details<br />ID";
                            break;
                        case "PredictedObservedTestsID":
                            gvPOTests.Columns[11].HeaderText = "PO Tests<br />ID";
                            break;
                    }
                    //get the index and details for the column we are sorting
                    if (headerCell.ContainingField.SortExpression == e.SortExpression)
                    {
                        sortColumnIndex = gvPOTests.HeaderRow.Cells.GetCellIndex(headerCell);
                    }
                }
                //gvApsimFiles.HeaderRow.Cells[columnIndex].Controls.Add(sortImage_ApsimFileList);
                if (_sortDirection_POTestsList == "ASC")
                {
                    gvPOTests.Columns[sortColumnIndex].HeaderText = gvPOTests.Columns[sortColumnIndex].HeaderText + "  ▲";
                }
                else
                {
                    gvPOTests.Columns[sortColumnIndex].HeaderText = gvPOTests.Columns[sortColumnIndex].HeaderText + "  ▼";
                }
                gvPOTests.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
                gvPOTests.DataBind();
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
                //this is the true/false PassedTest column
                if (e.Row.Cells[8].Text.Trim().ToLower() == "false")
                {
                    e.Row.ForeColor = Color.Red;
                }
                //this is the true/false IsImprovement column
                if (e.Row.Cells[9].Text.Trim().ToLower() == "true")
                {
                    e.Row.ForeColor = Color.Green;
                }

                //Activate the row click event
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvPOTests, "Select$" + e.Row.RowIndex);
                e.Row.Attributes["style"] = "cursor:pointer";
            }
        }

        protected void gvPOTests_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = gvPOTests.SelectedIndex;
            int predictedObservedtId = int.Parse(gvPOTests.Rows[index].Cells[10].Text);
            Response.Redirect("Details.aspx?PO_Id=" + predictedObservedtId);

        }

        #endregion


        #region Data Retreval and Binding

        private void BindPredictedObservedTestsDiffsDataTable()
        {
            if (POTestsDT == null)
            {
                if (Session["POTestsDT"] != null)
                {
                    POTestsDT = (DataTable)Session["POTestsDT"];
                }
            }
            if (POTestsDT == null)
            {
                BindPredictedObservedTestsDiffs();
            }
            //And display the combined data and display it in the grid
            gvPOTests.DataSource = POTestsDT;
            gvPOTests.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
            gvPOTests.DataBind();
        }

        private void BindPredictedObservedTestsDiffs()
        {
            //lblTests.Text = "Tests for " + variable;
            //get the data to be displayed in the chart and grid
            int pullRequestId = int.Parse(hfPullRequestID.Value.ToString());
            POTestsList = PredictedObservedDS.GetCurrentAcceptedTestsDiffsSubset(pullRequestId);

            POTestsDT = Genfuncs.ToDataTable(POTestsList);
            Session["POTestsDT"] = POTestsDT;
        }


        private void FilterPredictedObservedTestsDiffs(string filter)
        {
            //get back the original datatable (not the one we may have in the Session object)
            BindPredictedObservedTestsDiffs();

            if (filter.Length > 0)
            {
                DataView view = POTestsDT.DefaultView;
                view.RowFilter = " Test Like '%" + filter + "%' OR Variable Like '%" + filter + "%'  OR TableName Like '%" + filter + "%'  OR FileName Like '%" + filter + "%' ";
                POTestsDT = view.ToTable();
                Session["POTestsDT"] = POTestsDT;

                //And display the combined data and display it in the grid
            }
            gvPOTests.DataSource = POTestsDT;
            gvPOTests.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
            gvPOTests.DataBind();
        }

    
        protected void SetSortDirection(string gridname, string sortDirection)
        {
            if (sortDirection == "ASC")
            {
                _sortDirection_POTestsList = "DESC";
            }
            else
            {
                _sortDirection_POTestsList = "ASC";
            }
        }

        #endregion

    }
}