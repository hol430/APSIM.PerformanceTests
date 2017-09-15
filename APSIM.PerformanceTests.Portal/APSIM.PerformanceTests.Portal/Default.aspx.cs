using APSIM.PerformanceTests.Portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


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

                if (Request.QueryString["PULLREQUEST"] != null)
                {
                    int pullRequestId = int.Parse(Request.QueryString["PULLREQUEST"].ToString());
                    BindSimFilesGrid(pullRequestId);
                }
            }
            //if the Simulation File grid has data (ie after postback, then need to make sure the scolling will work
            if (gvSimFiles.Rows.Count > 0)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_SimFiles', 'ContentPlaceHolder1_gvSimFiles', 'GridHeaderDiv_SimFiles');</script>");
            }
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
                UpdatePullRequestStats("Accept", acceptlog);
            }
            else if (doUpdateStats == true)
            {
                UpdatePullRequestStats("Update", acceptlog);
            }

            Response.Redirect(Request.RawUrl);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
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
                    if (e.CommandName == "AcceptStats" && cellIndex == 7 && canUpdate == true)
                    {
                        lblTitle.Text = "Accept Stats Request";
                        lblPullRequestId2.Visible = false;
                        txtPullRequestId2.Visible = false;
                        lblDetails.Visible = true;
                        txtDetails.Visible = true;
                        txtPullRequestID.Text = gvApsimFiles.Rows[rowIndex].Cells[0].Text;
                        DateTime subDate = DateTime.Parse(gvApsimFiles.Rows[rowIndex].Cells[1].Text);
                        txtSubmitDate.Text = subDate.ToString("dd/MM/yyyy HH:mm");
                        txtSubmitPerson.Text = gvApsimFiles.Rows[rowIndex].Cells[2].Text;
                        txtAcceptDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                        this.ModalPopupExtender1.Show();
                    }
                    else if (e.CommandName == "UpdateStats")
                    {
                        lblTitle.Text = "Update Accepted Stats for this Pull Request";
                        lblPullRequestId2.Visible = true;
                        txtPullRequestId2.Visible = true;
                        lblDetails.Visible = false;
                        txtDetails.Visible = false;
                        txtPullRequestID.Text = gvApsimFiles.Rows[rowIndex].Cells[0].Text;
                        DateTime subDate = DateTime.Parse(gvApsimFiles.Rows[rowIndex].Cells[1].Text);
                        txtSubmitDate.Text = subDate.ToString("dd/MM/yyyy HH:mm");
                        txtSubmitPerson.Text = gvApsimFiles.Rows[rowIndex].Cells[2].Text;
                        txtAcceptDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                        this.ModalPopupExtender1.Show();

                    }
                    else if (e.CommandName == "CellSelect")
                    {
                        int pullRequestId = int.Parse(gvApsimFiles.Rows[rowIndex].Cells[0].Text);
                        BindSimFilesGrid(pullRequestId);
                    }
                }
            }
        }

        protected void gvApsimFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Show as green if 100% 
                if (e.Row.Cells[4].Text.Equals("100"))
                {
                    e.Row.ForeColor = Color.Green;
                }

                if (e.Row.Cells[3].Text.ToLower().Equals("true"))
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
                        if (cellIndex == 7)
                        {
                            if (e.Row.Cells[3].Text.ToLower().Equals("false"))
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
                        else if (cellIndex == 8)
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

        protected void gvSimFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells[3].Text.Equals("100"))
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
            int predictedObservedtId = int.Parse(gvSimFiles.Rows[index].Cells[0].Text);
            Response.Redirect("Details.aspx?PO_Id=" + predictedObservedtId);
        }

        #endregion


        #region Data Retreval and Binding

        private void BindApsimFilesGrid()
        {
            AcceptStatsLog acceptedPR = AcceptStatsLogDS.GetLatestAcceptedStatsLog();
            lblAcceptedDetails.Text = string.Format("Current Accepted Stats are for Pull Request Id {0}, submitted by {1}, accepted on {2}.", acceptedPR.PullRequestId, acceptedPR.SubmitPerson, acceptedPR.LogAcceptDate.ToString("dd-MMM-yyyy HH:MM"));

            ApsimFileList = ApsimFilesDS.GetPullRequestsWithStatus();
            gvApsimFiles.DataSource = ApsimFileList;
            gvApsimFiles.DataBind();
        }


        private void BindSimFilesGrid(int pullRequestId)
        {
            lblPullRequestId.Text = "Simulation Files for Pull Request Id: " + pullRequestId.ToString();

            List<vSimFile> simFiles = ApsimFilesDS.GetSimFilesByPullRequestID(pullRequestId);
            gvSimFiles.DataSource = simFiles;
            gvSimFiles.DataBind();

            ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_SimFiles', 'ContentPlaceHolder1_gvSimFiles', 'GridHeaderDiv_SimFiles');</script>");
        }


        private void BindSimFilesGrid(int pullRequestId, DateTime runDate)
        {
            lblPullRequestId.Text = string.Format("Simulation Files for Pull Request Id: {0}, on {1}.", pullRequestId.ToString(), runDate.ToString());

            List<vSimFile> simFiles = ApsimFilesDS.GetSimFilesByPullRequestIDandDate(pullRequestId, runDate);
            gvSimFiles.DataSource = simFiles;
            gvSimFiles.DataBind();

            ClientScript.RegisterStartupScript(this.GetType(), "CreateGridHeader", "<script>CreateGridHeader('GridDataDiv_SimFiles', 'ContentPlaceHolder1_gvSimFiles', 'GridHeaderDiv_SimFiles');</script>");
        }
        #endregion


        #region WebAPI Interaction

        private void UpdatePullRequestStats(string updateType, AcceptStatsLog apsimLog)
        {
            HttpClient httpClient = new HttpClient();

            string serviceUrl = ConfigurationManager.AppSettings["serviceAddress"].ToString() + "APSIM.PerformanceTests.Service/";
            httpClient.BaseAddress = new Uri(serviceUrl);
            //httpClient.BaseAddress = new Uri("http://www.apsim.info/APSIM.PerformanceTests.Service/");
#if DEBUG
            httpClient.BaseAddress = new Uri("http://localhost:53187/");
#endif
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = new HttpResponseMessage();
            if (updateType == "Accept")
            {
                response = httpClient.PostAsJsonAsync("api/acceptStats", apsimLog).Result;
            }
            else if (updateType == "Update")
            {
                response = httpClient.PostAsJsonAsync("api/updateStats", apsimLog).Result;
            }

            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
            }
        }
        #endregion
    }
}