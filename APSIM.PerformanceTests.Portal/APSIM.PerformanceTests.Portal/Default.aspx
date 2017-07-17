<%@ Page Language="C#" MasterPageFile="~/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.Default"
    Title="APSIM PerformanceTests|Home" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label ID="Label1" runat="server" CssClass="SectionTitles" Text="Pull Request Details"></asp:Label>
    
    <asp:GridView ID="gvApsimFiles" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle" 
        PageSize="10" AllowPaging="true" DataKeyNames="PullRequestId, RunDate"
        OnRowDataBound="gvApsimFiles_RowDataBound"
        OnSelectedIndexChanged="gvApsimFiles_SelectedIndexChanged"
        OnPageIndexChanging="gvApsimFiles_PageIndexChanging" >
        <HeaderStyle CssClass="GridViewHeaderStyle" />
        <Columns>
            <asp:BoundField DataField="PullRequestId" HeaderText="Pull Request Id" />
            <asp:BoundField DataField="RunDate" HeaderText="Run Date" />
            <asp:BoundField DataField="IsReleased" HeaderText="Is Released" />
        </Columns>
    </asp:GridView>


    <asp:UpdatePanel ID="upSimFiles" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <br />
            <asp:Label ID="lblPullRequestId" runat="server" CssClass="SectionTitles" Text=""></asp:Label>

            <div id="GridHeaderDiv">
            </div>

            <div id="GridDataDiv" onscroll="OnScrollFunctionSimFiles()">
                <asp:GridView ID="gvSimFiles" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle" 
                    OnRowDataBound="gvSimFiles_RowDataBound"
                    OnSelectedIndexChanged="gvSimFiles_SelectedIndexChanged" >
                    <HeaderStyle CssClass="GridViewHeaderStyle" />
                    <Columns>
                        <asp:BoundField DataField="PredictedObservedID" HeaderText="PO ID" 
                            ItemStyle-Width="60px" />
                        <asp:BoundField DataField="FileName" HeaderText="File Name" 
                            ItemStyle-Width="130px" />
                        <asp:BoundField DataField="PredictedObservedTableName" HeaderText="Predicted Observed<br />TableName" 
                             ItemStyle-Width="180px" HeaderStyle-Width="180px"  HtmlEncode="False"/>
                        <asp:BoundField DataField="PassedTests" HeaderText="Passed<br />Tests" 
                            ItemStyle-Width="60px" HeaderStyle-Width="60px"  HtmlEncode="False" />
                        <asp:BoundField DataField="FullFileName" HeaderText="Full FileName" 
                            ItemStyle-Width="230px" />
                    </Columns>
                </asp:GridView>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>


</asp:Content>
