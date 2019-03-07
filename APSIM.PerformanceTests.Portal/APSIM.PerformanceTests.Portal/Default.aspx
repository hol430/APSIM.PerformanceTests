<%@ Page Language="C#" MasterPageFile="~/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.Default"
    Title="APSIM PerformanceTests" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <table style="width: 100%">
        <tr>
            <td>
                <asp:Label ID="Label1" runat="server" CssClass="SectionTitles" Text="Pull Request Details"></asp:Label>
            </td>
<%--            <td style="width: 100px">
                <asp:Button ID="btnCompare" runat="server" Text="Compare Other Pull Requests" OnClick="btnCompare_Click" />
            </td>--%>
        </tr>
    </table>
    <asp:HiddenField ID="hfPullRequestId" runat="server" />
    <asp:HiddenField ID="hfAcceptedFileCount" runat="server" />

    <asp:GridView ID="gvApsimFiles" runat="server" AutoGenerateColumns="false"  PageSize="12" AllowPaging="true" AllowSorting="true" 
        DataKeyNames="PullRequestId, RunDate" 
        CssClass="Grid" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr"        
        OnPageIndexChanging="gvApsimFiles_PageIndexChanging" 
        OnRowCommand="gvApsimFiles_RowCommand"
        OnRowDataBound="gvApsimFiles_RowDataBound"
        OnSorting="gvApsimFiles_Sorting" >
        <Columns>
            <asp:BoundField DataField="PullRequestId" HtmlEncode="false" HeaderText="Pull<br />Req. Id" HeaderStyle-Width="80px" SortExpression="PullRequestId" />
            <asp:BoundField DataField="RunDate" HtmlEncode="false" HeaderText="Run Date" HeaderStyle-Width="200px" DataFormatString="{0:d-MMM-yyyy hh:mm tt}" SortExpression="RunDate" />
            <asp:BoundField DataField="SubmitDetails" HtmlEncode="false" HeaderText="Submit<br />Persons" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="85px" SortExpression ="SubmitDetails" />
            <asp:BoundField DataField="StatsAccepted"  HtmlEncode="false" HeaderText="Stats<br />Accepted" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="80px" />
            <asp:BoundField DataField="PercentPassed"  HtmlEncode="false" HeaderText="Percent<br />Passed" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="70px" SortExpression ="PercentPassed" />
            <asp:BoundField DataField="Total" HtmlEncode="false" HeaderText="Total<br />Files" ItemStyle-HorizontalAlign="Center"  HeaderStyle-Width="70px" SortExpression="Total"  />
            <asp:BoundField DataField="AcceptedPullRequestId" HtmlEncode="false" HeaderText="Accepted<br />PR Id" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="85px" />
            <asp:BoundField DataField="AcceptedRunDate" HtmlEncode="false" HeaderText="Accepted<br />Run Date" HeaderStyle-Width="180px" DataFormatString="{0:d-MMM-yyyy hh:mm tt}" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button ID="btnAcceptStats" runat="server" Text="Accept Stats"
                        Visible='<%# Eval("StatsAccepted").ToString().ToLowerInvariant().Equals("false") ? Eval("PercentPassed").ToString().ToLowerInvariant().Equals("100") ? false : true : false %>'
                    />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button ID="btnUpdateStats" runat="server" Text="Update Stats"
                        Visible='<%# Eval("StatsAccepted").ToString().ToLowerInvariant().Equals("false") ? Eval("PercentPassed").ToString().ToLowerInvariant().Equals("100") ? false : true : false %>'
                    />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:Label ID="lblAcceptedDetails" runat="server" Text="Accepted Pull Request Details"></asp:Label>
    <br />
    <br />
    <br />
    <table>
        <tr>
            <td>
<%--                <asp:Button ID="btnDifferences" runat="server" Text="Pull Request Tests - Differences" Visible="false" OnClick="btnDifferences_Click"/>--%>
                <asp:Button ID="btnTestsCharts" runat="server" Text="Pull Request Tests - Graphical Results" Visible="false" OnClick="btnTestsCharts_Click"/>
                <asp:Button ID="btnTestsGrids" runat="server" Text="Pull Request Tests - Tabulated Results" Visible="false" OnClick="btnTestsGrids_Click"/>
            </td>
        </tr>
    </table>

    <asp:UpdatePanel ID="upSimFiles" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <br />
            <asp:Label ID="lblPullRequestId" runat="server" CssClass="SectionTitles" Text=""></asp:Label>
            <br />
            <asp:Label ID="lblMissing" runat="server" CssClass="ScreenDetails, FailedTests" Text=""></asp:Label>

            <asp:GridView ID="gvSimFiles" runat="server" AutoGenerateColumns="false" PageSize="25" AllowPaging="true" AllowSorting="true" 
                DataKeyNames="PredictedObservedID" 
                CssClass="Grid2" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr"
                OnPageIndexChanging="gvSimFiles_PageIndexChanging"
                OnRowDataBound="gvSimFiles_RowDataBound"
                OnSelectedIndexChanged="gvSimFiles_SelectedIndexChanged"
                OnSorting="gvSimFiles_Sorting" >
                <Columns>
                    <asp:BoundField DataField="PredictedObservedID" HeaderText="PO ID" HeaderStyle-Width="70px" SortExpression="PredictedObservedID" />
                    <asp:BoundField DataField="FileName" HeaderText="File Name" HeaderStyle-Width="180px" SortExpression="FileName" />
                    <asp:BoundField DataField="PredictedObservedTableName" HtmlEncode="False" HeaderText="Predicted Observed<br />TableName" HeaderStyle-Width="230px" SortExpression="PredictedObservedTableName" />
                    <asp:BoundField DataField="PassedTests" HtmlEncode="False" HeaderText="Passed<br />Tests" HeaderStyle-Width="70px" ItemStyle-HorizontalAlign="Right" SortExpression="PassedTests" />
                    <asp:BoundField DataField="FullFileName" HeaderText="Full FileName" HeaderStyle-Width="380px" SortExpression="FullFileName" />
                    <asp:BoundField DataField="AcceptedPredictedObservedDetailsID" HtmlEncode="False"  HeaderText="Accepted<br />PO ID" HeaderStyle-Width="70px" />
                </Columns>
            </asp:GridView>

<%--        <div id="GridHeaderDiv_SimFiles">
            </div>
            <div id="GridDataDiv_SimFiles" onscroll="OnScrollFunction_SimFiles()">
                <asp:GridView ID="gvSimFiles" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle" 
                    AllowSorting="true"
                    OnRowDataBound="gvSimFiles_RowDataBound"
                    OnSelectedIndexChanged="gvSimFiles_SelectedIndexChanged" >
                    <HeaderStyle CssClass="GridViewHeaderStyle" />
                    <Columns>
                        <asp:BoundField DataField="PredictedObservedID" HeaderText="PO ID" ItemStyle-Width="60px" />
                        <asp:BoundField DataField="FileName" HeaderText="File Name" ItemStyle-Width="10px" />
                        <asp:BoundField DataField="PredictedObservedTableName" HtmlEncode="False" HeaderText="Predicted Observed<br />TableName" HeaderStyle-Width="180px" ItemStyle-Width="180px"  />
                        <asp:BoundField DataField="PassedTests" HtmlEncode="False" HeaderText="Passed<br />Tests" HeaderStyle-Width="60px" ItemStyle-HorizontalAlign="Right" ItemStyle-Width="60px"  />
                        <asp:BoundField DataField="FullFileName" HeaderText="Full FileName" ItemStyle-Width="230px" />
                        <asp:BoundField DataField="AcceptedPredictedObservedDetailsID" HtmlEncode="False"  HeaderText="Accepted<br />PO ID" ItemStyle-Width="60px" />
                    </Columns>
                </asp:GridView>
            </div>--%>

        </ContentTemplate>
    </asp:UpdatePanel>


    <asp:LinkButton ID="lnkDummy" runat="server"></asp:LinkButton>
    <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender1" BehaviorID="mpe" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlpopup" TargetControlID="lnkDummy"  CancelControlID="btnCancel" >
    </ajaxToolkit:ModalPopupExtender>

    <asp:Panel ID="pnlpopup" runat="server" CssClass="modalPopup" style="display:none" >
        <table >
            <tr>
                <td colspan="2"style="padding-bottom: 15px;">
                    <asp:Label ID="lblTitle" runat="server" CssClass="SectionTitles" Text="Accept Stats Request"></asp:Label>
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="Label4" runat="server" Width="140px" Text="Pull Request Id"></asp:Label></td>
                <td><asp:TextBox ID="txtPullRequestID" runat="server" Width="200px" Enabled="false" ></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="Label5" runat="server" Width="140px" Text="Submit Date"></asp:Label></td>
                <td><asp:TextBox ID="txtSubmitDate" runat="server" Width="200px" Enabled="false"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="Label6" runat="server" Width="140px" Text="Submit Person"></asp:Label></td>
                <td><asp:TextBox ID="txtSubmitPerson" runat="server" Width="200px"  Enabled="false"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="lblFileCount" runat="server" Width="140px" Text="File Count"></asp:Label></td>
                <td><asp:TextBox ID="txtFileCount" runat="server" Width="200px"  Enabled="false"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="Label7" runat="server" Width="140px" Text="Accept Date"></asp:Label></td>
                <td><asp:TextBox ID="txtAcceptDate" runat="server" Width="200px" Enabled="false"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="Label2" runat="server" Width="140px"  Text="User ID"></asp:Label></td>
                <td><asp:TextBox ID="txtName" runat="server" Width="200px"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="lblPullRequestId2" runat="server" Width="140px" Text="New PullRequest ID" Visible="false"></asp:Label></td>
                <td><asp:TextBox ID="txtPullRequestId2" runat="server" Width="80px" Visible="false"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="lblDetails" runat="server" Width="140px" Text="Details"></asp:Label></td>
                <td><asp:TextBox ID="txtDetails" runat="server" Width="300px"></asp:TextBox></td>
            </tr>
            <tr>
                <td colspan="2" style="padding-top: 15px;">
                    <asp:Button ID="btnOk" runat="server" Text="OK" OnClick="btnOk_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
                </td>
            </tr>
        </table>
    </asp:Panel>

</asp:Content>
