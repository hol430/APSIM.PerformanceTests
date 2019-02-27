<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChartsHelp.aspx.cs" Inherits="APSIM.PerformanceTests.Portal.ChartsHelp" %>

<asp:Content ID="content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>
    <div>
        <h1>Graphical Results Guide</h1>
        <ul>
            <li>Black for accepted results that are considered poor - when the absolute value of the statistic is greater than 1</li>
            <li>Grey for accepted results that are considered good - when the absolute value of the statistic is less than 1</li>
            <li>Orange for unchanged current results that are considered poor - when the absolute value of the statistic is greater than 1</li>
            <li>White for unchaged current results that are considered good - when the absolute value of the statistic is less than 1</li>
            <li>Red for current results which have worsened</li>
            <li>Green for current results which have improved</li>
        </ul>
    </div>
</body>
</html>
</asp:Content>