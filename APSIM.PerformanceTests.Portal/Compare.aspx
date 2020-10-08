<%@ Page Language="C#" EnableEventValidation="false" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Compare.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.Compare"
    Title="APSIM PerformanceTests Compare"   %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table style="width:90%">
                <tr>
                    <td style="width:50%;">
                        <asp:Label ID="Label1" runat="server" CssClass="ScreenLabel" Text="Select 1st Pull Request Id"></asp:Label>
                    </td>
                    <td style="width:50%;">
                        <asp:Label ID="Label2" runat="server" CssClass="ScreenLabel" Text="Select 2nd Pull Request Id"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox ID="txtPullRequest1" runat="server" CssClass="ScreenText" Width="400px" ReadOnly="true" />
                        <asp:Panel runat="server" ID="pnlApsimFiles1"  Style="display: none; visibility: hidden;"> 
                          <!-- GridView  goes here -->
                            <asp:GridView ID="gvApsimFiles1" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle_DropDown" 
                                DataKeyNames="PullRequestId, RunDate"
                                OnRowDataBound="gvApsimFiles1_RowDataBound"
                                OnSelectedIndexChanged="gvApsimFiles1_SelectedIndexChanged" >
                                <Columns>
                                    <asp:BoundField DataField="PullRequestId" HeaderText="Pull Request Id" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="RunDate" HtmlEncode="false" HeaderText="Run Date" ItemStyle-Width="220px" DataFormatString="{0:d MMMM, yyyy hh:mm tt}" />
                                    <asp:BoundField DataField="SubmitDetails" HtmlEncode="false" HeaderText="Submit<br />Persons" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px" />
                                    <asp:BoundField DataField="StatsAccepted"  HtmlEncode="false" HeaderText="Stats<br />Accepted" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="90px" />
                                    <asp:BoundField DataField="PercentPassed"  HtmlEncode="false" HeaderText="Percent<br />Passed" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px" />
                                    <asp:BoundField DataField="Total" HtmlEncode="false" HeaderText="Total<br />Files" ItemStyle-HorizontalAlign="Center"  ItemStyle-Width="80px" />
                                </Columns>
                                <HeaderStyle CssClass="GridViewHeaderStyle_DropDown" />
                            </asp:GridView>
                        </asp:Panel>
                        <ajaxToolkit:DropDownExtender ID="DropDownExtender1" runat="server" 
                            DropDownControlID="pnlApsimFiles1" 
                            TargetControlID="txtPullRequest1">
                        </ajaxToolkit:DropDownExtender>
                    </td>
                    <td>
                        <asp:TextBox ID="txtPullRequest2" runat="server" CssClass="ScreenText" Width="400px" ReadOnly="true" />
                        <asp:Panel runat="server" ID="pnlApsimFiles2"  Style="display: none; visibility: hidden;"> 
                          <!-- GridView  goes here -->
                            <asp:GridView ID="gvApsimFiles2" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle_DropDown" 
                                DataKeyNames="PullRequestId, RunDate"
                                OnRowDataBound="gvApsimFiles2_RowDataBound"
                                OnSelectedIndexChanged="gvApsimFiles2_SelectedIndexChanged">
                                <Columns>
                                    <asp:BoundField DataField="PullRequestId" HeaderText="Pull Request Id" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="RunDate" HtmlEncode="false" HeaderText="Run Date" ItemStyle-Width="220px" DataFormatString="{0:d MMMM, yyyy hh:mm tt}" />
                                    <asp:BoundField DataField="SubmitDetails" HtmlEncode="false" HeaderText="Submit<br />Persons" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px" />
                                    <asp:BoundField DataField="StatsAccepted"  HtmlEncode="false" HeaderText="Stats<br />Accepted" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="90px" />
                                    <asp:BoundField DataField="PercentPassed"  HtmlEncode="false" HeaderText="Percent<br />Passed" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px" />
                                    <asp:BoundField DataField="Total" HtmlEncode="false" HeaderText="Total<br />Files" ItemStyle-HorizontalAlign="Center"  ItemStyle-Width="80px" />
                                </Columns>
                                <HeaderStyle CssClass="GridViewHeaderStyle_DropDown" />
                            </asp:GridView>
                        </asp:Panel>
                        <ajaxToolkit:DropDownExtender ID="DropDownExtender2" runat="server" 
                            DropDownControlID="pnlApsimFiles2" 
                            TargetControlID="txtPullRequest2">
                        </ajaxToolkit:DropDownExtender>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        &nbsp;
                    </td>
                </tr>
            </table>
            <asp:Panel ID="pnlFileAndTableName" runat="server" Visible="false">
                <table>
                    <tr>
                        <td colspan="2">
                            <asp:Label ID="Label4" runat="server" CssClass="ScreenLabel" Text="Select FileName and PredictedObserved TableName"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:TextBox ID="txtSimFiles" runat="server" CssClass="ScreenText"  Width="500px" ReadOnly="true" />
                            <asp:Panel runat="server" ID="pnlSimFiles"  Style="display: none; visibility: hidden;"> 
                              <!-- GridView  goes here -->
                                <asp:GridView ID="gvSimFiles" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle_DropDown" 
                                    DataKeyNames="PredictedObservedID"
                                    OnRowDataBound="gvSimFiles_RowDataBound"
                                    OnSelectedIndexChanged="gvSimFiles_SelectedIndexChanged" >
                                    <Columns>
                                        <asp:BoundField DataField="PredictedObservedID" HeaderStyle-CssClass="invisibleColumn" ItemStyle-CssClass="invisibleColumn" />
                                        <asp:BoundField DataField="FileName" HeaderText="File Name" ItemStyle-Width="10px" />
                                        <asp:BoundField DataField="PredictedObservedTableName" HtmlEncode="False" HeaderText="Predicted Observed<br />TableName" HeaderStyle-Width="180px" ItemStyle-Width="180px"  />
                                        <asp:BoundField DataField="FullFileName" HeaderText="Full FileName" ItemStyle-Width="230px" />
                                    </Columns>
                                    <HeaderStyle CssClass="GridViewHeaderStyle_DropDown" />
                                </asp:GridView>
                            </asp:Panel>
                            <ajaxToolkit:DropDownExtender ID="DropDownExtender3" runat="server" 
                                DropDownControlID="pnlSimFiles" 
                                TargetControlID="txtSimFiles">
                            </ajaxToolkit:DropDownExtender>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            &nbsp;
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Panel ID="pnlPredictedObservedIds" runat="server" Visible="false" >
                <table>    
                    <tr>
                        <td style="width:33%">
                            <asp:Label ID="Label6" runat="server" CssClass="ScreenLabel" Text="1st PredictedObserved Id"></asp:Label>
                        </td>
                        <td style="width:33%">
                            <asp:Label ID="Label8" runat="server" CssClass="ScreenLabel" Text="2nd PredictedObserved Id"></asp:Label>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:TextBox ID="txtPredictedObservedID1" runat="server" CssClass="ScreenText"  Width="100px" ReadOnly="true" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtPredictedObservedID2" runat="server" CssClass="ScreenText"  Width="100px" ReadOnly="true" />
                        </td>
                        <td>
                            <asp:Button ID="btnCompare" runat="server" Text=" Compare " OnClick="btnCompare_Click" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Panel ID="pnlVariables" runat="server" Visible="false" >
                <table style="padding-top: 15px; " >
                    <tr>
                        <td>
                            <asp:Timer ID="LoadTimer" runat="server" OnTick="LoadTimer_Tick" Interval="600" ></asp:Timer>
                            <asp:Label ID="Label3" runat="server" CssClass="ScreenText" Text="Select a Variable: " ></asp:Label>&nbsp;&nbsp;
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlVariables" runat="server" AutoPostBack="True" width="300px"
                                DataTextField ="Name" DataValueField="Value" CssClass="ScreenLabel"
                                OnSelectedIndexChanged="ddlVariables_SelectedIndexChanged" >
                            </asp:DropDownList>
                        </td>
                    </tr>
                </table>

            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
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
