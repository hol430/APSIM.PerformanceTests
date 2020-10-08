<%@ Page Language="C#" MasterPageFile="~/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Difference.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.Difference"
    Title="APSIM PerformanceTests Difference"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table style="width: 100%">
        <tr>
            <td colspan="2"> &nbsp;
            </td>
            <td style="text-align:right; width: 300px" >
                <asp:Button ID="btnBack" runat="server" Text="Return to Pull Request / Simulation Files" OnClick="btnBack_Click"/>
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <asp:Label ID="lblPullRequest" runat="server" CssClass="SectionTitles" Text="Pull Request Id: "></asp:Label><br />
                <asp:HiddenField ID="hfPullRequestID" runat="server" />
            </td>
        </tr>
        <tr>
            <td style="width: 500px">
                <asp:Label ID="Label8" runat="server" CssClass="SectionTitles" Text="Predicted Observed Tests with Differences"></asp:Label>
            </td>
            <td style="width: auto">
                <asp:Label ID="Label6" runat="server" CssClass="ScreenLabel" Text="Page Size"></asp:Label>
                <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="True" 
                    OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" >
                    <asp:ListItem Text="100" Value="100" ></asp:ListItem>
                    <asp:ListItem Selected="True" Text="200" Value="200" ></asp:ListItem>
                    <asp:ListItem Text="300" Value="300" ></asp:ListItem>
                    <asp:ListItem Text="400" Value="400" ></asp:ListItem>
                    <asp:ListItem Text="500" Value="500" ></asp:ListItem>
                </asp:DropDownList>
                &nbsp;&nbsp;
            </td>
            <td style="width: auto">
                <asp:Label ID="Label1" runat="server" CssClass="ScreenLabel" Text="Filter"></asp:Label>
                <asp:TextBox ID="txtSearch" CssClass="ScreenText" runat="server" AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" ></asp:TextBox>
            </td>
        </tr>
    </table>

    <asp:GridView ID="gvPOTests" runat="server" AutoGenerateColumns="false" AllowPaging="true" AllowSorting="true" 
        DataKeyNames="FileName, Variable, Test"  
        CssClass="Grid2" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr"        
        OnPageIndexChanging="gvPOTests_PageIndexChanging" 
        OnRowCommand="gvPOTests_RowCommand"
        OnSorting="gvPOTests_Sorting"
        OnRowDataBound="gvPOTests_RowDataBound"
        OnSelectedIndexChanged="gvPOTests_SelectedIndexChanged"  >
        <Columns>
            <asp:BoundField DataField="FileName" HtmlEncode="False" HeaderText="Apsim<br />File Name" HeaderStyle-Width="200px" SortExpression="FileName" />
            <asp:BoundField DataField="TableName" HtmlEncode="False" HeaderText="Predicted Observed<br /> TableName"  HeaderStyle-Width="150px" SortExpression="TableName" />
            <asp:BoundField DataField="Variable" HeaderText="Variable" HeaderStyle-Width="80px" SortExpression="Variable" />
            <asp:BoundField DataField="Test" HeaderText="Test" HeaderStyle-Width="70px" SortExpression="Test" />
            <asp:BoundField DataField="Accepted" HtmlEncode="false" HeaderText="Accepted" ItemStyle-HorizontalAlign="Right" HeaderStyle-Width="80px" DataFormatString="{0:0.000000}" SortExpression="Accepted"  />
            <asp:BoundField DataField="Current" HtmlEncode="false" HeaderText="Current" ItemStyle-HorizontalAlign="Right" HeaderStyle-Width="80px" DataFormatString="{0:0.000000}" SortExpression="Current" />
            <asp:BoundField DataField="Difference" HeaderText="Difference" ItemStyle-HorizontalAlign="Right" HeaderStyle-Width="80px" DataFormatString="{0:0.000000}" SortExpression="Difference" />
            <asp:BoundField DataField="DifferencePercent" HtmlEncode="false" HeaderText="Difference<br />Percent" ItemStyle-HorizontalAlign="Right" HeaderStyle-Width="80px" DataFormatString="{0:0.0}%" SortExpression="DifferencePercent" />
            <asp:BoundField DataField="PassedTest" HtmlEncode="False" HeaderText="Passed<br />Test" HeaderStyle-Width="70px" SortExpression="PassedTest" />
            <asp:BoundField DataField="IsImprovement" HtmlEncode="False" HeaderText="Is<br />Improvement" HeaderStyle-Width="80px" SortExpression="IsImprovement" />
            <asp:BoundField DataField="PredictedObservedDetailsID" HtmlEncode="False" HeaderText="PO Details<br /> ID"  HeaderStyle-Width="60px" SortExpression="PredictedObservedDetailsID"/>
            <asp:BoundField DataField="PredictedObservedTestsID" HtmlEncode="False" HeaderText="PO Tests<br /> ID"  HeaderStyle-Width="60px" SortExpression="PredictedObservedDetailsID" Visible="false" />
        </Columns>
    </asp:GridView>
</asp:Content>
