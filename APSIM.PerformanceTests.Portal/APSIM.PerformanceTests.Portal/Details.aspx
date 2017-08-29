<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.Details"
    Title="APSIM PerformanceTests Details"  %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table style="width: 100%">
        <tr>
            <td style="width:auto">
                <asp:Label ID="Label4" runat="server" CssClass="SectionTitles" Text="Predicted Observed Details:"></asp:Label><br />
                <asp:HiddenField ID="hfPullRequestID" runat="server" />
                <asp:HiddenField ID="hfPredictedObservedID" runat="server" />
            </td>
            <td style="width: 100px">
                <asp:Button ID="btnBack" runat="server" Text="Return to Pull Request / Simulation Files" OnClick="btnBack_Click" />
            </td>
        </tr>
        <tr>
            <td colspan="2" style="padding-left: 25px;">
                <asp:Label ID="lblPullRequest" runat="server" CssClass="SectionTitles" Text="lblPullRequest"></asp:Label><br />
                <asp:Label ID="lblApsimFile" runat="server" CssClass="SectionTitles" Text="lblApsimFilelblApsimFile"></asp:Label><br />
                <asp:Label ID="lblPOTableName" runat="server" CssClass="SectionTitles" Text="lblPOTableName"></asp:Label>
            </td>
        </tr>
    </table>
    <table>
        <tr>
            <td><br />
                <asp:Label ID="lblTests" runat="server" CssClass="ScreenText" Text="Predicted Observed Tests:"></asp:Label>
                <div id="GridHeaderDiv_POTests">
                </div>
                <div id="GridDataDiv_POTests" onscroll="OnScrollFunction_POTests()" >
                    <asp:GridView ID="gvPOTests" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle" 
                        OnRowDataBound="gvPOTests_RowDataBound" >
                        <HeaderStyle CssClass="GridViewHeaderStyle" />
                        <Columns>
                            <asp:BoundField DataField="Variable" 
                                HeaderText="Variable" HeaderStyle-Width="200px" 
                                ItemStyle-Width="80px" />
                            <asp:BoundField DataField="Test" 
                                HeaderText="Test" HeaderStyle-Width="120px" 
                                ItemStyle-Width="80px"  />
                            <asp:BoundField DataField="Accepted" HtmlEncode="false"
                                HeaderText="Accepted" HeaderStyle-Width="120px" 
                                ItemStyle-HorizontalAlign="Right" ItemStyle-Width="80px"  />
                            <asp:BoundField DataField="Current"  HtmlEncode="false"
                                HeaderText="Current" HeaderStyle-Width="120px" 
                                ItemStyle-HorizontalAlign="Right" ItemStyle-Width="80px"/>
                            <asp:BoundField DataField="Difference" 
                                HeaderText="Difference" HeaderStyle-Width="100px" 
                                ItemStyle-HorizontalAlign="Right" ItemStyle-Width="80px"  />
                            <asp:BoundField DataField="PassedTest" HtmlEncode="False" 
                                HeaderText="Passed<br />Test" HeaderStyle-Width="100px" 
                                ItemStyle-Width="80px"  />
                        </Columns>
                    </asp:GridView>
                </div>
                <br />
            </td>
        </tr>
    </table>
    <table style="width: 95%; padding-top: 15px; background-color: lightgray; border: 1px, solid, darkgray; " >
        <tr>
            <td>
                <asp:Label ID="Label1" runat="server" CssClass="SectionTitles" Text="Predicted Observed Simulation Values Drilldown"></asp:Label>&nbsp;&nbsp;
                <br />
                &nbsp;
            </td>
        </tr>
    </table>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table style="padding-top: 15px; " >
                <tr>
                    <td>
                        <asp:Timer ID="LoadTimer" runat="server" OnTick="LoadTimer_Tick" Interval="600" ></asp:Timer>
                        <asp:Label ID="Label2" runat="server" CssClass="ScreenText" Text="Select a Variable: " ></asp:Label>&nbsp;&nbsp;
                    </td>
                    <td>
                        <asp:DropDownList ID="ddlVariables" runat="server" AutoPostBack="True" width="300px"
                            DataTextField ="Name" DataValueField="Value" CssClass="ScreenLabel"
                            OnSelectedIndexChanged="ddlVariables_SelectedIndexChanged" >
                        </asp:DropDownList>
                    </td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdatePanel ID="UpdatePanel2" runat="server" >
        <ContentTemplate>

            <br />
            <asp:Label ID="lblError" runat="server" Text="lblError" Visible="false" ></asp:Label>
            <asp:Chart ID="chartPODetails" runat="server" Height="500px" Width="900px" >
                <Series>
                    <asp:Series Name="Accepted" ChartArea="ChartArea1" 
                        ChartType="Point" MarkerColor="255, 128, 0" MarkerSize="12" MarkerStyle="Circle"
                        IsVisibleInLegend="true" Tooltip="Observed: #VALX, Predicted: #VALY" Legend="Legend1" ></asp:Series>
                    <asp:Series Name="Current" ChartArea="ChartArea1" 
                        ChartType="Point" MarkerColor="Navy" MarkerSize="6" MarkerStyle="Diamond"
                        IsVisibleInLegend="true" Tooltip="Observed: #VALX, Predicted: #VALY" Legend="Legend1" ></asp:Series>
                    <asp:Series Name="Slope" ChartArea="ChartArea1" ChartType="Line" Color="Black" IsVisibleInLegend="false"  ></asp:Series>
                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
                </ChartAreas>
                <Legends>
                    <asp:Legend DockedToChartArea="ChartArea1" Font="Microsoft Sans Serif, 10pt" IsDockedInsideChartArea="False" IsTextAutoFit="False" Name="Legend1">
                    </asp:Legend>
                </Legends>
                <Titles>
                    <asp:Title Name="chartTitle" Text="Current vs Accepted" Font="Microsoft Sans Serif, 13pt, style=Bold" ></asp:Title>
                    <asp:Title Name="xAxisTitle" Text="Observed Values" Docking="Bottom" Font="Microsoft Sans Serif, 11pt, style=Bold" ></asp:Title>
                    <asp:Title Name="yAxisTitle" Text="Predicted Values" Docking="Left" Font="Microsoft Sans Serif, 11pt, style=Bold" ></asp:Title>
                </Titles>
            </asp:Chart>
            <br />

            <asp:Label ID="lblValues" runat="server" CssClass="ScreenText" Text="Current and Accepted values for Variable:" ></asp:Label>
            <!-- *** Begin Header Table *** -->
            <div id="GridHeaderDiv_POValues">
            </div>
            <!-- *** End Header Table *** -->
            <div id="GridDataDiv_POValues" onscroll="OnScrollFunction_POValues()" >
                <asp:GridView ID="gvPOValues" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle_POValues"
                    OnRowDataBound="gvPOValues_RowDataBound"  >
                    <HeaderStyle CssClass="GridViewHeaderStyle_POValues" />
					<Columns>
                        <asp:BoundField DataField="SimulationName" HeaderStyle-Width="200px"  HeaderText="&nbsp;Simulation Name" ItemStyle-Width="200px" />
                        <asp:BoundField DataField="CurrentPredictedValue" HtmlEncode="False" HeaderStyle-Width="100px" HeaderText="Current<br />Predicted<br />Value" ItemStyle-HorizontalAlign="Right" ItemStyle-Width="100px"  />
                        <asp:BoundField DataField="CurrentObservedValue" HtmlEncode="False" HeaderText ="Current<br />Observed<br />Value" HeaderStyle-Width="100px" ItemStyle-HorizontalAlign="Right" ItemStyle-Width="100px" />
                        <asp:BoundField DataField="AcceptedPredictedValue" HtmlEncode="False" HeaderText ="Accepted<br />Predicted<br />Value" HeaderStyle-Width="100px" ItemStyle-HorizontalAlign="Right" ItemStyle-Width="100px" />
                        <asp:BoundField DataField="AcceptedObservedValue" HtmlEncode="False" HeaderText ="Accepted<br />Observed<br />Value"  HeaderStyle-Width="100px" ItemStyle-HorizontalAlign="Right" ItemStyle-Width="100px" />
                        <asp:BoundField DataField="TableName" HeaderText ="Table Name"  HeaderStyle-Width="120px" ItemStyle-Width="120px" />
                        <asp:BoundField DataField="MatchNames" HeaderText ="Match Names"  HeaderStyle-Width="100px" ItemStyle-Width="100px" />
                        <asp:BoundField DataField="MatchValues"  HeaderText ="Match Values"  HeaderStyle-Width="200px" ItemStyle-Width="200px" />
                    </Columns>

                </asp:GridView>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
