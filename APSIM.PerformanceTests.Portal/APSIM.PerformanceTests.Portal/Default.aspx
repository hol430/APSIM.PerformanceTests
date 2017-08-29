<%@ Page Language="C#" MasterPageFile="~/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.Default"
    Title="APSIM PerformanceTests Home" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label ID="Label1" runat="server" CssClass="SectionTitles" Text="Pull Request Details"></asp:Label>
    
    <asp:GridView ID="gvApsimFiles" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle" 
        PageSize="10" AllowPaging="true" DataKeyNames="PullRequestId, RunDate"
        OnPageIndexChanging="gvApsimFiles_PageIndexChanging" 
        OnRowCommand="gvApsimFiles_RowCommand"
        OnRowDataBound="gvApsimFiles_RowDataBound" >
        <HeaderStyle CssClass="GridViewHeaderStyle" />
        <RowStyle CssClass="GridViewRowStyle" />
        <Columns>
            <asp:BoundField DataField="PullRequestId" HeaderText="Pull Request Id" ItemStyle-Width="100px" />
            <asp:BoundField DataField="RunDate" HtmlEncode="false" HeaderText="Run Date" ItemStyle-Width="220px" DataFormatString="{0:d MMMM, yyyy hh:mm tt}" />
            <asp:BoundField DataField="SubmitDetails" HtmlEncode="false" HeaderText="Submit<br />Persons" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px" />
            <asp:BoundField DataField="StatsAccepted"  HtmlEncode="false" HeaderText="Stats<br />Accepted" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="90px" />
            <asp:BoundField DataField="PercentPassed"  HtmlEncode="false" HeaderText="Percent<br />Passed" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px" />
            <asp:BoundField DataField="Total" HtmlEncode="false" HeaderText="Total<br />Files" ItemStyle-HorizontalAlign="Center"  ItemStyle-Width="80px" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button ID="btnAcceptStats" runat="server" Text="Accept Stats"
                        Visible='<%# Eval("StatsAccepted").ToString().ToLowerInvariant().Equals("false") %>'
                    />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <br />
    <asp:Label ID="lblAcceptedDetails" runat="server" CssClass="ScreenLabel" Text="Accepted Pull Request Details"></asp:Label>

    <asp:UpdatePanel ID="upSimFiles" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <br />
            <asp:Label ID="lblPullRequestId" runat="server" CssClass="SectionTitles" Text=""></asp:Label>

            <div id="GridHeaderDiv_SimFiles">
            </div>
            <div id="GridDataDiv_SimFiles" onscroll="OnScrollFunction_SimFiles()">
                <asp:GridView ID="gvSimFiles" runat="server" AutoGenerateColumns="false" CssClass="GridViewStyle" 
                    OnRowDataBound="gvSimFiles_RowDataBound"
                    OnSelectedIndexChanged="gvSimFiles_SelectedIndexChanged" >
                    <HeaderStyle CssClass="GridViewHeaderStyle" />
                    <Columns>
                        <asp:BoundField DataField="PredictedObservedID" 
                            HeaderText="PO ID" 
                            ItemStyle-Width="60px" />
                        <asp:BoundField DataField="FileName" 
                            HeaderText="File Name" 
                            ItemStyle-Width="10px" />
                        <asp:BoundField DataField="PredictedObservedTableName" HtmlEncode="False"
                            HeaderText="Predicted Observed<br />TableName" HeaderStyle-Width="180px" 
                            ItemStyle-Width="180px"  />
                        <asp:BoundField DataField="PassedTests" HtmlEncode="False" 
                            HeaderText="Passed<br />Tests" HeaderStyle-Width="60px" 
                            ItemStyle-HorizontalAlign="Right" ItemStyle-Width="60px"  />
                        <asp:BoundField DataField="FullFileName" 
                            HeaderText="Full FileName" 
                            ItemStyle-Width="230px" />
                    </Columns>
                </asp:GridView>
            </div>
            <asp:LinkButton ID="lnkDummy" runat="server"></asp:LinkButton>
            <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender1" BehaviorID="mpe" runat="server" BackgroundCssClass="modalBackground"
                PopupControlID="pnlpopup" TargetControlID="lnkDummy"  CancelControlID="btnCancel" >
            </ajaxToolkit:ModalPopupExtender>

            <asp:Panel ID="pnlpopup" runat="server" CssClass="modalPopup" style="display:none" >
                <table >
                    <tr>
                        <td colspan="2"style="padding-bottom: 15px;">
                            <asp:Label ID="Label17" runat="server" CssClass="SectionTitles" Text="Accept Stats Request"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label4" runat="server" Width="100px" Text="Pull Request Id"></asp:Label></td>
                        <td><asp:TextBox ID="txtPullRequestID" runat="server" Width="200px" Enabled="false" ></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label5" runat="server" Width="100px" Text="Submit Date"></asp:Label></td>
                        <td><asp:TextBox ID="txtSubmitDate" runat="server" Width="200px" Enabled="false"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label6" runat="server" Width="100px" Text="Submit Person"></asp:Label></td>
                        <td><asp:TextBox ID="txtSubmitPerson" runat="server" Width="200px"  Enabled="false"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label2" runat="server" Width="100px"  Text="User ID"></asp:Label></td>
                        <td><asp:TextBox ID="txtName" runat="server" Width="200px"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label3" runat="server" Width="100px" Text="Details"></asp:Label></td>
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

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
