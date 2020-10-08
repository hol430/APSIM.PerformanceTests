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
    public partial class RenameTable : System.Web.UI.Page
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
                if (txtNewTableName.Text.Trim().Length <= 0)
                {
                    throw new Exception("Please enter a New TableName.");
                }
                if (ddlTableName.SelectedItem.Text.Trim() == txtNewTableName.Text.Trim())
                {
                    throw new Exception("The New Tablename cannot be the same as the old TableName.");
                }

                PORename rename = new PORename();
                rename.SubmitUser = txtUserName.Text;
                rename.Type = "TableRename";
                rename.FileName = ddlApsimFile.SelectedItem.Text;
                rename.TableName = ddlTableName.SelectedItem.Text;
                rename.NewTableName = txtNewTableName.Text;

                WebAP_Interactions.RenamePredictedObservedTable(rename);
            }
            catch (Exception ex)
            {
                lblErrors.Text = "Error:  " + ex.Message.ToString();
            }
        }

        protected void btnUpdateTableName_Click(object sender, EventArgs e)
        {
            lblTitle.Text = "Predicted Observed Table Rename";
            this.ModalPopupExtender1.Show();
        }

        protected void btnUpdateVariableName_Click(object sender, EventArgs e)
        {
            lblTitle.Text = "Predicted Observed Variable Rename";
            this.ModalPopupExtender1.Show();
        }

        protected void ddlApsimFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fileName = ddlApsimFile.SelectedItem.Text;
            BindTableNames(fileName);
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

        private void BindTableNames(string fileName)
        {
            List<vVariable> tableNameList = PredictedObservedDS.GetDistinctTableNames(fileName);
            vVariable selectItem = new vVariable();
            selectItem.Name = "Select a Table";
            selectItem.Value = "0";
            tableNameList.Insert(0, selectItem);

            ddlTableName.DataSource = tableNameList;
            ddlTableName.DataBind();
        }
        #endregion



    }
}