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
        <h2>Heatmap Explanation</h2>
        <p>
            Unchanged results are a shade of grey, ranging from black if the stat is considered poor (RSR > <asp:Label ID="lblRSRThreshold" runat="server" Text="Label"></asp:Label> or NSE < <asp:Label ID="lblNSEThreshold" runat="server" Text="Label"></asp:Label>), to white if the stat is considered good.
        </p>
        <p>
            Improved results are a shade of green; darker for minor improvements, and brighter for more significant improvements.
        </p>
        <p>
            Results which have worsened are a shade of red; the darker the shade, the more significantly the result has worsened.
        </p>
        <h2>Bar Chart Guide</h2>
        <ul>
            <li>Black for accepted results that are considered poor - when the absolute value of the statistic is greater than 1</li>
            <li>Grey for accepted results that are considered good - when the absolute value of the statistic is less than 1</li>
            <li>Orange for results from this pull request if they are unchanged but considered poor - when the absolute value of the statistic is greater than 1</li>
            <li>White for results from this pull request if they are unchaged but considered good - when the absolute value of the statistic is less than 1</li>
            <li>Red for current results which have worsened</li>
            <li>Green for current results which have improved</li>
        </ul>
    </div>
</body>
</html>
</asp:Content>