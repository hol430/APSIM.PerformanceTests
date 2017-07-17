<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.Details"
    Title="APSIM PerformanceTests|Details"  %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label ID="lblPredictedObserved" runat="server" CssClass="SectionTitles" Text="Predicted Observed Details for ID:"></asp:Label>
    <asp:HiddenField ID="hfPullRequestID" runat="server" />

    <table style="padding-top: 15px;">
        <tr>
            <td>
                <asp:Label ID="Label2" runat="server" CssClass="ScreenText" Text="Select a Variable: "></asp:Label>&nbsp;&nbsp;
            </td>
            <td>
                <asp:DropDownList ID="ddlVariables" runat="server" AutoPostBack="True" width="300px"
                    DataTextField ="Name" DataValueField="Value" CssClass="ScreenLabel"
                    OnSelectedIndexChanged="ddlVariables_SelectedIndexChanged" >
                </asp:DropDownList>
            </td>
        </tr>
    </table>


    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <br />
            <asp:Label ID="lblError" runat="server" Text="lblError" Visible="false"></asp:Label>
            <asp:Chart ID="chartPODetails" runat="server" Height="500px" Width="900px" >
                <Series>
                    <asp:Series Name="Accepted" ChartArea="ChartArea1" 
                        ChartType="Point" MarkerColor="255, 128, 0" MarkerSize="12" MarkerStyle="Circle"
                        IsVisibleInLegend="true" Tooltip="Observed: #VALX, Predicted: #VALY" Legend="Legend1" ></asp:Series>
                    <asp:Series Name="Current" ChartArea="ChartArea1" 
                        ChartType="Point" MarkerColor="Navy" MarkerSize="6" MarkerStyle="Diamond"
                        IsVisibleInLegend="true" Tooltip="Observed: #VALX, Predicted: #VALY" Legend="Legend1" ></asp:Series>
                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
                </ChartAreas>
                <Legends>
                    <asp:Legend DockedToChartArea="ChartArea1" Font="Microsoft Sans Serif, 10pt" IsDockedInsideChartArea="False" IsTextAutoFit="False" Name="Legend1">
                    </asp:Legend>
                </Legends>
                <Titles>
                    <asp:Title Name="chartTitle" Text="Current vs Accepted" Font="Microsoft Sans Serif, 13pt, style=Bold">
                    </asp:Title>
                    <asp:Title Name="xAxisTitle" Text="Observed Values" Docking="Bottom" Font="Microsoft Sans Serif, 11pt, style=Bold" >
                    </asp:Title>
                    <asp:Title Name="yAxisTitle" Text="Predicted Values" Docking="Left" Font="Microsoft Sans Serif, 11pt, style=Bold" >
                    </asp:Title>
                </Titles>
            </asp:Chart>

            <div id="GridHeaderDiv">
            </div>

            <div id="GridDataDiv_POValues" onscroll="OnScrollFunctionPOValues()">
                <asp:GridView ID="gvPODetails" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle" 
                    OnRowDataBound="gvPODetails_RowDataBound"
                    >
                    <HeaderStyle CssClass="GridViewHeaderStyle" />
                    <Columns>
                        <asp:BoundField DataField="TableName" HeaderText="Table Name" 
                            ItemStyle-Width="120px" />
                        <asp:BoundField DataField="SimulationName" HeaderText="Simulation Name" 
                            ItemStyle-Width="150px" />
                        <asp:BoundField DataField="CurrentPredictedValue" HeaderText="Current<br />Predicted<br />Value" 
                             ItemStyle-Width="80px" HeaderStyle-Width="80px"  HtmlEncode="False" />
                        <asp:BoundField DataField="CurrentObservedValue" HeaderText="Current<br />Observed<br />Value" 
                             ItemStyle-Width="80px" HeaderStyle-Width="80px"  HtmlEncode="False" />
                        <asp:BoundField DataField="CurrentDifference" HeaderText="Current<br />Difference"
                             ItemStyle-Width="80px" HeaderStyle-Width="80px"  HtmlEncode="False" />
                        <asp:BoundField DataField="AcceptedPredictedValue" HeaderText="Accepted<br />Predicted<br />Value" 
                             ItemStyle-Width="80px" HeaderStyle-Width="80px"  HtmlEncode="False" />
                        <asp:BoundField DataField="AcceptedObservedValue" HeaderText="Accepted<br />Observed<br />Value" 
                             ItemStyle-Width="80px" HeaderStyle-Width="80px"  HtmlEncode="False" />
                        <asp:BoundField DataField="AcceptedDifference" HeaderText="Accepted<br />Difference" 
                             ItemStyle-Width="80px" HeaderStyle-Width="80px"  HtmlEncode="False" />
                        <asp:BoundField DataField="DifferenceDifference" HeaderText="Difference<br />Difference" 
                             ItemStyle-Width="80px" HeaderStyle-Width="80px"  HtmlEncode="False" />
                        <asp:BoundField DataField="MatchNames" HeaderText="Match Names" 
                            ItemStyle-Width="100px" />
                        <asp:BoundField DataField="MatchValues" HeaderText="Match Values" 
                            ItemStyle-Width="200px" />
                    </Columns>
                </asp:GridView>
                </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
