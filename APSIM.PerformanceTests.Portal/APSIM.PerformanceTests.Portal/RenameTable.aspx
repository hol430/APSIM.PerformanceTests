<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RenameTable.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.RenameTable"
    Title="APSIM Performance Tests Rename Table"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <asp:Panel ID="Panel1" runat="server"  CssClass="dropDown" Width="80%">
                <table style="width: 100%">
                    <tr>
                        <td colspan="4">
                            <asp:Label ID="Label4" runat="server" CssClass="SectionTitles" Text="Predicted Observed TableName Rename"></asp:Label><br />
                            <br />
                        </td>
                    </tr>
                    <tr>
                        <td style="padding-left: 10px;">
                            <asp:Label ID="lblApsimFileName" runat="server" CssClass="ScreenLabel" Text="Apsim FileName"></asp:Label><br />
                        </td>
                        <td>
                            <asp:Label ID="lblTableName" runat="server" CssClass="ScreenLabel" Text="Predicted Observed TableName"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblNewTableName" runat="server" CssClass="ScreenLabel" Text="New TableName"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding-left: 10px;">
                            <asp:DropDownList ID="ddlApsimFile" runat="server" AutoPostBack="true" width="250px"
                                DataTextField ="Name" DataValueField="Value" CssClass="dropDown"
                                OnSelectedIndexChanged="ddlApsimFile_SelectedIndexChanged"></asp:DropDownList>
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlTableName" runat="server"  width="250px"  CssClass="dropDown"
                                DataTextField ="Name" DataValueField="Value"  ></asp:DropDownList>
                        </td>
                        <td>
                            <asp:TextBox ID="txtNewTableName" runat="server" CssClass="dropDown" width="250px"></asp:TextBox>
                        </td>
                    </tr>
                   <tr>
                        <td colspan="3"><br />
                            <asp:Label ID="lblErrors" runat="server" CssClass="FailedTests" Text=""></asp:Label>
                        </td>
                   </tr>
                   <tr>
                        <td colspan="3">
                            <asp:Button ID="btnUpdateTableName" runat="server"  CssClass="dropDown" Text="Update Predicted Observed TableName" Onclick="btnUpdateTableName_Click" />
                        </td>
                   </tr>
                </table>
            </asp:Panel>
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
                    <asp:Label ID="lblTitle" runat="server" CssClass="SectionTitles" Text="Predicted Observed Table Rename "></asp:Label>
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="Label2" runat="server" Width="140px"  Text="User ID"></asp:Label></td>
                <td><asp:TextBox ID="txtUserName" runat="server" Width="200px"></asp:TextBox></td>
            </tr>
            <tr>
                <td colspan="2"style="padding-bottom: 15px;">
                    <p>Please confirm the rename by entering your User Id.</p>
                </td>
            </tr>
            <tr>
                <td colspan="2" style="padding-top: 15px;">
                    <asp:Button ID="btnOk" runat="server" Text="OK" OnClick="btnOk_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel"/>
                </td>
            </tr>
        </table>
    </asp:Panel>

</asp:Content>
