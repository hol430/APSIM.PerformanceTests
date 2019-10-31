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
using System.Text;

namespace APSIM.PerformanceTests.Portal
{
    public partial class Default : System.Web.UI.Page
    {
        #region Constants and variables

        //constants used for the gvApsimFiles grid
        const int colPullRequestId = 0;
        const int colRunDate = 1;
        const int colSubmitDetails = 2;
        const int colStatsAccepted = 3;
        const int colPercentPassed = 4;
        const int colTotal = 5;
        const int colAcceptedPullRequestId = 6;
        const int colAcceptedRunDate = 7;
        const int colAcceptStats = 8;
        const int colUpdateStats = 9;

        //constants used for the gvSimFiles grid
        const int colPredictedObservedID = 0;
        const int colFileName = 1;
        const int colPredictedObservedTableName = 2;
        const int colPassedTests = 3;
        const int colFullFileName = 4;
        const int colAcceptedPredictedObservedDetailsID = 5;


        private List<vApsimFile> ApsimFileList;
        private List<vSimFile> SimFilesList;

        private DataTable ApsimFileDT;
        private DataTable SimFilesDT;

        System.Web.UI.WebControls.Image sortImage_ApsimFileList = new System.Web.UI.WebControls.Image();
        System.Web.UI.WebControls.Image sortImage_SimFilesList = new System.Web.UI.WebControls.Image();

        public string SortDireaction_ApsimFileList
        {
            get
            {
                if (ViewState["SortDireaction_ApsimFileList"] == null)
                    return string.Empty;
                else
                    return ViewState["SortDireaction_ApsimFileList"].ToString();
            }
            set
            {
                ViewState["SortDireaction_ApsimFileList"] = value;
            }
        }
        private string _sortDirection_ApsimFileList;

        public string SortDireaction_SimFilesList
        {
            get
            {
                if (ViewState["SortDireaction_SimFilesList"] == null)
                    return string.Empty;
                else
                    return ViewState["SortDireaction_SimFilesList"].ToString();
            }
            set
            {
                ViewState["SortDireaction_SimFilesList"] = value;
            }
        }
        private string _sortDirection_SimFilesList;
        #endregion


        #region Page and Control Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindApsimFilesGrid();

                if (Request.QueryString["PULLREQUEST"] != null)
                {
                    
                    int pullRequestId = int.Parse(Request.QueryString["PULLREQUEST"].ToString());
                    hfPullRequestId.Value = pullRequestId.ToString();

                    BindSimFilesGrid(pullRequestId);
                }
            }
            //if the Simulation File grid has data (ie after postback, then need to make sure the scolling will work
            //if (gvSimFiles.Rows.Count > 0)
            //{
            //    ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_SimFiles', 'ContentPlaceHolder1_gvSimFiles', 'GridHeaderDiv_SimFiles');</script>");
            //}
        }

        protected void btnCompare_Click(object sender, EventArgs e)
        {
            Response.Redirect("Compare.aspx");
        }


        protected void btnOk_Click(object sender, EventArgs e)
        {
            AcceptStatsLog acceptlog = new AcceptStatsLog();
            acceptlog.PullRequestId = int.Parse(txtPullRequestID.Text);
            acceptlog.SubmitDate = DateTime.ParseExact(txtSubmitDate.Text, "dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);

            acceptlog.SubmitPerson = txtSubmitPerson.Text;
            string fileInfo = txtFileCount.Text.Trim();
            int posn = fileInfo.IndexOf('.');  
            if (posn > 0)
            {
                acceptlog.FileCount = int.Parse(fileInfo.Substring(0, posn));
            }
            else
            {
                acceptlog.FileCount = int.Parse(txtFileCount.Text);
            }

            acceptlog.LogPerson = txtName.Text;
            acceptlog.LogReason = txtDetails.Text;
            acceptlog.LogAcceptDate = DateTime.ParseExact(txtAcceptDate.Text, "dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
            acceptlog.LogStatus = true;

            bool doAcceptStats = false;
            bool doUpdateStats = false;
            if (lblTitle.Text.StartsWith("Update"))
            {
                int pullRequestId2 = 0;
                if (int.TryParse(txtPullRequestId2.Text, out pullRequestId2) == true)
                {
                    acceptlog.StatsPullRequestId = pullRequestId2;
                    acceptlog.LogReason = "Update 'Accepted' Stats to Pull Request Id: " + pullRequestId2.ToString();
                    acceptlog.LogStatus = false;
                    doUpdateStats = true;
                }
            }
            else
            {
                doAcceptStats = true;
            }

            txtName.Text = string.Empty;
            txtDetails.Text = string.Empty;
            txtPullRequestId2.Text = string.Empty;
            this.ModalPopupExtender1.Hide();

            if (doAcceptStats == true)
            {
                WebAP_Interactions.UpdatePullRequestStats("Accept", acceptlog);
            }
            else if (doUpdateStats == true)
            {
                WebAP_Interactions.UpdatePullRequestStats("Update", acceptlog);
            }

            Response.Redirect(Request.RawUrl);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
        }

        protected void btnDifferences_Click(object sender, EventArgs e)
        {
            string pullrequestId = hfPullRequestId.Value.ToString();
            Response.Redirect(string.Format("Difference.aspx?PULLREQUEST={0}", pullrequestId));
        }

        protected void btnTestsCharts_Click(object sender, EventArgs e)
        {
            string pullrequestId = hfPullRequestId.Value.ToString();
            Response.Redirect(string.Format("TestsCharts.aspx?PULLREQUEST={0}", pullrequestId));
        }
        protected void btnTestsGrids_Click(object sender, EventArgs e)
        {
            string pullrequestId = hfPullRequestId.Value.ToString();
            Response.Redirect(string.Format("TestsGrids.aspx?PULLREQUEST={0}", pullrequestId));
        }


        protected void gvApsimFiles_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvApsimFiles.PageIndex = e.NewPageIndex;
            BindApsimFilesGrid();
        }

        protected void gvApsimFiles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Don't interfere with other commands.  We may not have any now, but this is another safe-code strategy.
            if (e.CommandName == "CellSelect" || e.CommandName == "AcceptStats" || e.CommandName == "UpdateStats")
            {
                // Unpack the arguments.
                String[] arguments = ((String)e.CommandArgument).Split(new char[] { ',' });

                // More safe coding: Don't assume there are at least 2 arguments. (And ignore when there are more.)
                if (arguments.Length >= 2)
                {
                    // And even more safe coding: Don't assume the arguments are proper int values.
                    int rowIndex = -1, cellIndex = -1;
                    bool canUpdate = false;
                    int.TryParse(arguments[0], out rowIndex);
                    int.TryParse(arguments[1], out cellIndex);
                    bool.TryParse(arguments[2], out canUpdate);

                    // Use the rowIndex to select the Row, like Select would do.
                    if (rowIndex > -1 && rowIndex < gvApsimFiles.Rows.Count)
                    {
                        gvApsimFiles.SelectedIndex = rowIndex;
                    }

                    //here we either update the Update Panel (if the user clicks only anything OTHER THAN our'Button'
                    //or we process the UpdatePullRequest as Merged
                    if (e.CommandName == "AcceptStats" && cellIndex == 8 && canUpdate == true)
                    {
                        lblTitle.Text = "Accept Stats Request";
                        lblPullRequestId2.Visible = false;
                        txtPullRequestId2.Visible = false;
                        lblDetails.Visible = true;
                        txtDetails.Visible = true;
                        lblFileCount.Visible = true;
                        txtFileCount.Visible = true;

                        txtPullRequestID.Text = gvApsimFiles.Rows[rowIndex].Cells[colPullRequestId].Text;
                        DateTime subDate = DateTime.Parse(gvApsimFiles.Rows[rowIndex].Cells[colRunDate].Text);
                        txtSubmitDate.Text = subDate.ToString("dd/MM/yyyy HH:mm");
                        txtSubmitPerson.Text = gvApsimFiles.Rows[rowIndex].Cells[colSubmitDetails].Text;
                        txtAcceptDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                        int acceptedFileCount = Int32.Parse(hfAcceptedFileCount.Value.ToString());
                        int currentFilecount = Int32.Parse(gvApsimFiles.Rows[rowIndex].Cells[colTotal].Text);
                        if (acceptedFileCount != currentFilecount)
                        {
                            txtFileCount.Text = string.Format("{0}. This does not match 'Accepted' file count of {1}.", currentFilecount.ToString(), acceptedFileCount.ToString());
                            txtFileCount.CssClass = "FailedTests";
                            txtFileCount.Width = Unit.Pixel(320);
                            pnlpopup.Height = Unit.Pixel(300);
                        }
                        else
                        {
                            txtFileCount.Text = currentFilecount.ToString();
                            //txtFileCount.CssClass = "Reset";
                            txtFileCount.Width = Unit.Pixel(200);
                            pnlpopup.Height = Unit.Pixel(270);
                        }
                        this.ModalPopupExtender1.Show();
                    }
                    else if (e.CommandName == "UpdateStats")
                    {
                        lblTitle.Text = "Update Accepted Stats for this Pull Request";
                        lblPullRequestId2.Visible = true;
                        txtPullRequestId2.Visible = true;
                        lblDetails.Visible = false;
                        txtDetails.Visible = false;
                        //lblFileCount.Visible = false;
                        //txtFileCount.Visible = false;
                        int acceptedFileCount = Int32.Parse(hfAcceptedFileCount.Value.ToString());
                        int currentFilecount = Int32.Parse(gvApsimFiles.Rows[rowIndex].Cells[colTotal].Text);
                        if (acceptedFileCount != currentFilecount)
                        {
                            txtFileCount.Text = string.Format("{0}. This does not match 'Accepted' file count of {1}.", currentFilecount.ToString(), acceptedFileCount.ToString());
                            txtFileCount.CssClass = "FailedTests";
                            txtFileCount.Width = Unit.Pixel(320);
                            pnlpopup.Height = Unit.Pixel(300);
                        }
                        else
                        {
                            txtFileCount.Text = currentFilecount.ToString();
                            //txtFileCount.CssClass = "Reset";
                            txtFileCount.Width = Unit.Pixel(200);
                            pnlpopup.Height = Unit.Pixel(270);
                        }

                        txtPullRequestID.Text = gvApsimFiles.Rows[rowIndex].Cells[colPullRequestId].Text;
                        DateTime subDate = DateTime.Parse(gvApsimFiles.Rows[rowIndex].Cells[colRunDate].Text);
                        txtSubmitDate.Text = subDate.ToString("dd/MM/yyyy HH:mm");
                        txtSubmitPerson.Text = gvApsimFiles.Rows[rowIndex].Cells[colSubmitDetails].Text;
                        txtAcceptDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                        pnlpopup.Height = Unit.Pixel(260);
                        this.ModalPopupExtender1.Show();

                    }
                    else if (e.CommandName == "CellSelect")
                    {
                        int pullRequestId = int.Parse(gvApsimFiles.Rows[rowIndex].Cells[colPullRequestId].Text);
                        DateTime subDate = DateTime.Parse(gvApsimFiles.Rows[rowIndex].Cells[colRunDate].Text);
                        int acceptedPullRequestId = int.Parse(gvApsimFiles.Rows[rowIndex].Cells[colAcceptedPullRequestId].Text);
                        int passPercent = int.Parse(gvApsimFiles.Rows[rowIndex].Cells[colPercentPassed].Text);
                        BindSimFilesGrid(pullRequestId, subDate, acceptedPullRequestId, passPercent);
                    }
                }
            }
        }

        protected void gvApsimFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells[colPercentPassed].Text.Equals("100"))
                {
                    e.Row.ForeColor = Color.Green;
                }
                if (e.Row.Cells[colStatsAccepted].Text.ToLower().Equals("true"))
                {
                    e.Row.ForeColor = Color.Green;
                    e.Row.Font.Bold = true;
                }

                //Active cell click events on individual cells, instead of the row
                foreach (TableCell cell in e.Row.Cells)
                {
                    // Although we already know this should be the case, make safe code. Makes copying for reuse a lot easier.
                    if (cell is DataControlFieldCell)
                    {
                        int cellIndex = e.Row.Cells.GetCellIndex(cell);
                        bool canUpdate = false;
                        // if we are binding the 'Button' column, and the "StatsAccepted' is false, then whe can Update the Merge Status.
                        if (cellIndex == 8)
                        {
                            if (e.Row.Cells[colStatsAccepted].Text.ToLower().Equals("false"))
                            {
                                canUpdate = true;
                                Button db = (Button)e.Row.Cells[cellIndex].FindControl("btnAcceptStats");
                                if (db != null)
                                {
                                    db.OnClientClick = "return confirm('Are you certain you want to Accept the Stats for this Pull Request?');";
                                    db.CommandName = "AcceptStats";
                                    db.CommandArgument = String.Format("{0},{1},{2}", e.Row.RowIndex, cellIndex, canUpdate);
                                }
                            }
                        }
                        else if (cellIndex == 9)
                        {
                            canUpdate = true;
                            Button db = (Button)e.Row.Cells[cellIndex].FindControl("btnUpdateStats");
                            if (db != null)
                            {
                                db.OnClientClick = "return confirm('Are you certain you want to Update the Stats for this Pull Request?');";
                                db.CommandName = "UpdateStats";
                                db.CommandArgument = String.Format("{0},{1},{2}", e.Row.RowIndex, cellIndex, canUpdate);
                            }
                        }
                        else
                        {
                            // Put the link on the cell.
                            cell.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvApsimFiles, String.Format("CellSelect${0},{1},{2}", e.Row.RowIndex, cellIndex, canUpdate));
                            e.Row.Attributes["style"] = "cursor:pointer";
                            // Register for event validation: This will keep ASP from giving nasty errors from getting events from controls that shouldn't be sending any.
                            Page.ClientScript.RegisterForEventValidation(gvApsimFiles.UniqueID, String.Format("CellSelect${0},{1},{2}", e.Row.RowIndex, cellIndex, canUpdate));
                        }
                    }
                }
            }
        }

        protected void gvApsimFiles_Sorting(object sender, GridViewSortEventArgs e)
        {
            SetSortDirection("gvApsimFiles", SortDireaction_ApsimFileList);

            if (ApsimFileDT == null) 
            {
                if (Session["ApsimFileDT"] != null)
                {
                    ApsimFileDT = (DataTable)Session["ApsimFileDT"];
                }
            }

            if (ApsimFileDT != null)
            {
                //Sort the data.
                ApsimFileDT.DefaultView.Sort = e.SortExpression + " " + _sortDirection_ApsimFileList;
                gvApsimFiles.DataSource = ApsimFileDT;

                gvApsimFiles.DataBind();
                SortDireaction_ApsimFileList = _sortDirection_ApsimFileList;

                int sortColumnIndex = 0;
                foreach (DataControlFieldHeaderCell headerCell in gvApsimFiles.HeaderRow.Cells)
                {
                    //Make sure we are displaying the correct header for all columns
                    switch (headerCell.ContainingField.SortExpression)
                    {
                        case "PullRequestId":
                            gvApsimFiles.Columns[colPullRequestId].HeaderText = "Pull<br />Req. Id";
                            break;
                        case "RunDate":
                            gvApsimFiles.Columns[colRunDate].HeaderText = "Run Date";
                            break;
                        case "SubmitDetails":
                            gvApsimFiles.Columns[colSubmitDetails].HeaderText = "Submit<br />Persons";
                            break;
                        case "StatsAccepted":
                            gvApsimFiles.Columns[colStatsAccepted].HeaderText = "Stats<br />Accepted";
                            break;
                        case "PercentPassed":
                            gvApsimFiles.Columns[colPercentPassed].HeaderText = "Percent<br />Passed";
                            break;
                        case "Total":
                            gvApsimFiles.Columns[colTotal].HeaderText = "Total<br />Files";
                            break;
                        case "AcceptedPullRequestId":
                            gvApsimFiles.Columns[colAcceptedPullRequestId].HeaderText = "Accepted<br />PR Id";
                            break;
                        case "AcceptedRunDate":
                            gvApsimFiles.Columns[colAcceptedRunDate].HeaderText = "Accepted<br />Run Date";
                            break;
                    }
                    //get the index and details for the column we are sorting
                    if (headerCell.ContainingField.SortExpression == e.SortExpression)
                    {
                        sortColumnIndex = gvApsimFiles.HeaderRow.Cells.GetCellIndex(headerCell);
                    }
                }
                if (_sortDirection_ApsimFileList == "ASC")
                {
                    gvApsimFiles.Columns[sortColumnIndex].HeaderText = gvApsimFiles.Columns[sortColumnIndex].HeaderText + "  ▲";
                }
                else
                {
                    gvApsimFiles.Columns[sortColumnIndex].HeaderText = gvApsimFiles.Columns[sortColumnIndex].HeaderText + "  ▼";
                }
                gvApsimFiles.DataBind();

            }
        }


        protected void gvSimFiles_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSimFiles.PageIndex = e.NewPageIndex;
            if (SimFilesDT == null)
            {
                if (Session["SimFilesDT"] != null)
                {
                    SimFilesDT = (DataTable)Session["SimFilesDT"];
                }
            }

            if (SimFilesDT != null)
            {
                //Sort the data.
                gvSimFiles.DataSource = SimFilesDT;
                gvSimFiles.DataBind();
            }
        }


        protected void gvSimFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells[colPassedTests].Text.Equals("100"))
                {
                    e.Row.ForeColor = Color.Green;
                }
                //Activate the row click event
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvSimFiles, "Select$" + e.Row.RowIndex);
                e.Row.Attributes["style"] = "cursor:pointer";
            }
        }

        protected void gvSimFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = gvSimFiles.SelectedIndex;
            int predictedObservedtId = int.Parse(gvSimFiles.Rows[index].Cells[colPredictedObservedID].Text);
            Response.Redirect("PODetails.aspx?PO_Id=" + predictedObservedtId);
        }

        protected void gvSimFiles_Sorting(object sender, GridViewSortEventArgs e)
        {
            SetSortDirection("gvSimFiles", SortDireaction_SimFilesList);

            if (SimFilesDT == null)
            {
                if (Session["SimFilesDT"] != null)
                {
                    SimFilesDT = (DataTable)Session["SimFilesDT"];
                }
            }

            if (SimFilesDT != null)
            {
                //Sort the data.
                SimFilesDT.DefaultView.Sort = e.SortExpression + " " + _sortDirection_SimFilesList;
                gvSimFiles.DataSource = SimFilesDT;

                gvSimFiles.DataBind();
                SortDireaction_SimFilesList = _sortDirection_SimFilesList;

                int sortColumnIndex = 0;
                foreach (DataControlFieldHeaderCell headerCell in gvSimFiles.HeaderRow.Cells)
                {
                    //Make sure we are displaying the correct header for all columns
                    switch (headerCell.ContainingField.SortExpression)
                    {
                        case "PredictedObservedID":
                            gvSimFiles.Columns[colPredictedObservedID].HeaderText = "PO ID";
                            break;
                        case "FileName":
                            gvSimFiles.Columns[colFileName].HeaderText = "File Name";
                            break;
                        case "PredictedObservedTableName":
                            gvSimFiles.Columns[colPredictedObservedTableName].HeaderText = "Predicted Observed<br />TableName";
                            break;
                        case "PassedTests":
                            gvSimFiles.Columns[colPassedTests].HeaderText = "Passed<br />Tests";
                            break;
                        case "FullFileName":
                            gvSimFiles.Columns[colFullFileName].HeaderText = "Full FileName";
                            break;
                        case "AcceptedPredictedObservedDetailsID":
                            gvSimFiles.Columns[colAcceptedPredictedObservedDetailsID].HeaderText = "Accepted<br />PO ID";
                            break;
                    }
                    //get the index and details for the column we are sorting
                    if (headerCell.ContainingField.SortExpression == e.SortExpression)
                    {
                        sortColumnIndex = gvSimFiles.HeaderRow.Cells.GetCellIndex(headerCell);
                    }
                }
                if (_sortDirection_SimFilesList == "ASC")
                {
                    gvSimFiles.Columns[sortColumnIndex].HeaderText = gvSimFiles.Columns[sortColumnIndex].HeaderText + "  ▲";
                }
                else
                {
                    gvSimFiles.Columns[sortColumnIndex].HeaderText = gvSimFiles.Columns[sortColumnIndex].HeaderText + "  ▼";
                }
                gvSimFiles.DataBind();

            }

        }

        #endregion


        #region Data Retreval and Binding

        private void BindApsimFilesGrid()
        {
            ApsimFileList = ApsimFilesDS.GetPullRequestsWithStatus();
            ApsimFileDT = Genfuncs.ToDataTable(ApsimFileList);

            Session["ApsimFileDT"] = ApsimFileDT;
            gvApsimFiles.DataSource = ApsimFileDT;
            gvApsimFiles.DataBind();

            AcceptStatsLog acceptedPR = AcceptStatsLogDS.GetLatestAcceptedStatsLog();
            if (acceptedPR != null)
            {
                lblAcceptedDetails.Text = string.Format("Current Accepted Stats are for Pull Request Id {0}, submitted by {1}, accepted on {2}.", acceptedPR.PullRequestId, acceptedPR.SubmitPerson, acceptedPR.LogAcceptDate.ToString("dd-MMM-yyyy HH:MM tt"));
                hfAcceptedFileCount.Value = acceptedPR.FileCount.ToString();
            }
        }


        private void BindSimFilesGrid(int pullRequestId)
        {
            lblMissing.Text = string.Empty;
            hfPullRequestId.Value = pullRequestId.ToString();

            lblPullRequestId.Text = "Simulation Files for Pull Request Id: " + pullRequestId.ToString();

            //btnDifferences.Visible = true;
            //btnDifferences.Text = "Pull Request " + pullRequestId.ToString() + " Tests - Differences " ;

            btnTestsCharts.Visible = true;
            btnTestsCharts.Text = "Pull Request " + pullRequestId.ToString() + " Tests - Graphical Results";
            btnTestsGrids.Visible = true;
            btnTestsGrids.Text = "Pull Request " + pullRequestId.ToString() + " Tests - Tabulated Results";

            SimFilesList = ApsimFilesDS.GetSimFilesByPullRequestID(pullRequestId);
            SimFilesDT = Genfuncs.ToDataTable(SimFilesList);

            Session["SimFilesDT"] = SimFilesDT;
            gvSimFiles.DataSource = SimFilesDT;
            gvSimFiles.DataBind();

            //ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_SimFiles', 'ContentPlaceHolder1_gvSimFiles', 'GridHeaderDiv_SimFiles');</script>");
        }

        private void BindSimFilesGrid(int pullRequestId, DateTime runDate, int acceptPullRequestId, int PercentPassed)
        {
            //how many files are in the accepted Pull Request Set
            //what happens if they do not match
            lblMissing.Text = string.Empty;
            hfPullRequestId.Value = pullRequestId.ToString();
            if (acceptPullRequestId > 0)
            {
                List<string> missingTables = ApsimFilesDS.GetMissingTables(pullRequestId, acceptPullRequestId);
                if (missingTables != null && missingTables.Count > 0)
                    lblMissing.Text = "Missing FileName.TableName(s): " + string.Join(",", missingTables) + ".";

                List<string> newTables = ApsimFilesDS.GetNewTables(pullRequestId, acceptPullRequestId);
                if (newTables != null && newTables.Count > 0)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendLine("New predicted/observed tables have been added by this pull request:");
                    message.AppendLine(string.Join(",", newTables));
                    lblNewFiles.Text = message.ToString();
                }
            }

            lblPullRequestId.Text = "Simulation Files for Pull Request Id: " + pullRequestId.ToString();
            if (PercentPassed == 100)
            {
                //btnDifferences.Visible = false;
                btnTestsCharts.Visible = true;
                btnTestsGrids.Visible = true;
            }
            else
            {
                //btnDifferences.Visible = true;
                //btnDifferences.Text = "Pull Request " + pullRequestId.ToString() + " Tests - Differences ";

                btnTestsCharts.Visible = true;
                btnTestsCharts.Text = "Pull Request " + pullRequestId.ToString() + " Tests - Graphical Results";
                btnTestsGrids.Visible = true;
                btnTestsGrids.Text = "Pull Request " + pullRequestId.ToString() + " Tests - Tabulated Results";
            }

            SimFilesList = ApsimFilesDS.GetSimFilesByPullRequestIDandDate(pullRequestId, runDate);
            SimFilesDT = Genfuncs.ToDataTable(SimFilesList);

            Session["SimFilesDT"] = SimFilesDT;

            gvSimFiles.DataSource = SimFilesDT;
            gvSimFiles.DataBind();

            //ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_SimFiles', 'ContentPlaceHolder1_gvSimFiles', 'GridHeaderDiv_SimFiles');</script>");
        }


        protected void SetSortDirection(string gridname, string sortDirection)
        {
            if (sortDirection == "ASC")
            {
                if (gridname == "gvApsimFiles")
                {
                    _sortDirection_ApsimFileList = "DESC";
                }
                else if (gridname == "gvSimFiles")
                {
                    _sortDirection_SimFilesList = "DESC";
                }
            }
            else
            {
                if (gridname == "gvApsimFiles")
                {
                    _sortDirection_ApsimFileList = "ASC";
                }
                else if (gridname == "gvSimFiles")
                {
                    _sortDirection_SimFilesList = "ASC";
                }
            }
        }
        #endregion

    }
}