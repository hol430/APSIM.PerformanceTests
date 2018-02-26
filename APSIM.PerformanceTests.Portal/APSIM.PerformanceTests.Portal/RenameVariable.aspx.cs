using APSIM.PerformanceTests.Portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace APSIM.PerformanceTests.Portal
{
    public partial class RenameVariable : System.Web.UI.Page
    {
        #region Constants and variables

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //Get a list of distinct Apsim FileNames
                BindApsimFileNames();
            }
        }

        #endregion


        #region Page and Control Events

        protected void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlApsimFile.SelectedItem.Value == "0")
                {
                    throw new Exception("Please select a valid Apsim FileName.");
                }
                if (ddlTableName.SelectedItem.Value == "0")
                {
                    throw new Exception("Please select a valid TableName.");
                }
                if (ddlVariableName.SelectedItem.Value == "0")
                {
                    throw new Exception("Please select a valid Variable .");
                }
                if (txtNewVariableName.Text.Trim().Length <= 0)
                {
                    throw new Exception("Please enter a New Variable Name.");
                }
                if (ddlVariableName.SelectedItem.Text.Trim() == txtNewVariableName.Text.Trim())
                {
                    throw new Exception("The New Variable Name cannot be the same as the old Variable Name.");
                }

                PORename rename = new PORename();
                rename.SubmitUser = txtUserName.Text;

                rename.Type = "VariableRename";
                rename.FileName = ddlApsimFile.SelectedItem.Text;
                rename.TableName = ddlTableName.SelectedItem.Text;
                rename.VariableName = ddlVariableName.SelectedItem.Text;
                rename.NewVariableName = txtNewVariableName.Text;

                RenamePredictedObservedTable(rename);
            }
            catch (Exception ex)
            {
                lblErrors.Text = "Error:  " + ex.Message.ToString();
            }
        }

        protected void btnUpdateVariableName_Click(object sender, EventArgs e)
        {
            this.ModalPopupExtender1.Show();
        }

        protected void ddlApsimFile_SelectedIndexChanged1(object sender, EventArgs e)
        {
            string fileName = ddlApsimFile.SelectedItem.Text;
            BindTableNames("Variable", fileName);
        }

        protected void ddlTableName_SelectedIndexChanged1(object sender, EventArgs e)
        {
            string fileName = ddlApsimFile.SelectedItem.Text;
            string tableName = ddlTableName.SelectedItem.Text;
            BindVariableNames(fileName, tableName);
        }
        #endregion



        #region Data Retreval and Binding

        private void BindApsimFileNames()
        {
            List<vVariable> fileNameList = ApsimFilesDS.GetDistinctApsimFileNames();
            vVariable selectItem = new vVariable();
            selectItem.Name = "Select a File";
            selectItem.Value = "0";
            fileNameList.Insert(0, selectItem);

            ddlApsimFile.DataSource = fileNameList;
            ddlApsimFile.DataBind();
        }

        private void BindTableNames(string renameType, string fileName)
        {
            List<vVariable> tableNameList = PredictedObservedDS.GetDistinctTableNames(fileName);
            vVariable selectItem = new vVariable();
            selectItem.Name = "Select a Table";
            selectItem.Value = "0";
            tableNameList.Insert(0, selectItem);

            ddlTableName.DataSource = tableNameList;
            ddlTableName.DataBind();
        }

        private void BindVariableNames(string fileName, string tableName)
        {
            List<vVariable> variableNameList = PredictedObservedDS.GetDistinctVariableNames(fileName, tableName);
            vVariable selectItem = new vVariable();
            selectItem.Name = "Select a Variable";
            selectItem.Value = "0";
            variableNameList.Insert(0, selectItem);
            ddlVariableName.DataSource = variableNameList;
            ddlVariableName.DataBind();
        }

        #endregion


        #region WebAPI Interaction


        private void RenamePredictedObservedTable(PORename objRename)
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
            response = httpClient.PostAsJsonAsync("api/PORename", objRename).Result;
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
            }

        }

        #endregion

    }
}