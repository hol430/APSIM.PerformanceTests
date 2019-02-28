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
        <h2>Bar Chart Guide</h2>
        <ul>
            <li>Black for accepted results that are considered poor - when the absolute value of the statistic is greater than 1</li>
            <li>Grey for accepted results that are considered good - when the absolute value of the statistic is less than 1</li>
            <li>Orange for results from this pull request if they are unchanged but considered poor - when the absolute value of the statistic is greater than 1</li>
            <li>White for results from this pull request if they are unchaged but considered good - when the absolute value of the statistic is less than 1</li>
            <li>Red for current results which have worsened</li>
            <li>Green for current results which have improved</li>
        </ul>
        <h2>Heatmap Explanation</h2>
        <p>
            The heatmap only displays statistics for data in this pull request. The colour scheme is the same as for the bar charts, except that black and grey do not apply since the accepted stats are not shown.
        </p>
    </div>
</body>
</html>
</asp:Content>