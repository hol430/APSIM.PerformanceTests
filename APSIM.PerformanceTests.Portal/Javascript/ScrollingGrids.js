
//*****************************************************************************
//****** This is used to scroll the Simulation Files datagrid            ******
//*****************************************************************************
function OnScrollFunction_SimFiles() {
    var div = document.getElementById('GridDataDiv_SimFiles');
    var div2 = document.getElementById('GridHeaderDiv_SimFiles');
    //****** Scrolling HeaderDiv along with DataDiv ******
    div2.scrollLeft = div.scrollLeft;
    return false;
}

function OnScrollFunction_POValues() {
    var div = document.getElementById('GridDataDiv_POValues');
    var div2 = document.getElementById('GridHeaderDiv_POValues');
    //****** Scrolling HeaderDiv along with DataDiv ******
    div2.scrollLeft = div.scrollLeft;
    return false;
}

function OnScrollFunction_POTests() {
    var div = document.getElementById('GridDataDiv_POTests');
    var div2 = document.getElementById('GridHeaderDiv_POTests');
    //****** Scrolling HeaderDiv along with DataDiv ******
    div2.scrollLeft = div.scrollLeft;
    return false;
}

function CreateGridHeader(DataDiv, GridView1, HeaderDiv) {
    var DataDivObj = document.getElementById(DataDiv);
    var DataGridObj = document.getElementById(GridView1);
    var HeaderDivObj = document.getElementById(HeaderDiv);

    //********* Creating new table which contains the header row ***********
    var HeadertableObj = HeaderDivObj.appendChild(document.createElement('table'));
    DataDivObj.style.paddingTop = '0px';
    var DataDivWidth = DataDivObj.clientWidth;
    DataDivObj.style.width = '5500px';

    //********** Setting the style of Header Div as per the Data Div ************
    HeaderDivObj.className = DataDivObj.className;
    HeaderDivObj.style.cssText = DataDivObj.style.cssText;

    //**** Making the Header Div scrollable. *****
    HeaderDivObj.style.overflow = 'auto';

    //*** Hiding the horizontal scroll bar of Header Div ****
    //*** this is because we have to scroll the Div along with the DataDiv.
    HeaderDivObj.style.overflowX = 'hidden';

    //**** Hiding the vertical scroll bar of Header Div ****
    HeaderDivObj.style.overflowY = 'hidden';
    HeaderDivObj.style.height = DataGridObj.rows[0].clientHeight + 'px';

    //**** Removing any border between Header Div and Data Div ****
    HeaderDivObj.style.borderBottomWidth = '0px';

    //********** Setting the style of Header Table as per the GridView ************
    HeadertableObj.className = DataGridObj.className;

    //**** Setting the Headertable css text as per the GridView css text
    HeadertableObj.style.cssText = DataGridObj.style.cssText;
    HeadertableObj.border = '1px';
    HeadertableObj.rules = 'all';
    HeadertableObj.cellPadding = DataGridObj.cellPadding;
    HeadertableObj.cellSpacing = DataGridObj.cellSpacing;

    //********** Creating the new header row **********
    var Row = HeadertableObj.insertRow(0);
    Row.className = DataGridObj.rows[0].className;
    Row.style.cssText = DataGridObj.rows[0].style.cssText;
    Row.style.fontWeight = 'bold';

    //******** This loop will create each header cell *********
    for (var iCntr = 0; iCntr < DataGridObj.rows[0].cells.length; iCntr++) {
        var spanTag = Row.appendChild(document.createElement('td'));
        spanTag.innerHTML = DataGridObj.rows[0].cells[iCntr].innerHTML;
        var width = 0;

        //****** Setting the width of Header Cell **********
        if (spanTag.clientWidth > DataGridObj.rows[1].cells[iCntr].clientWidth) {
            width = spanTag.clientWidth;
        }
        else {
            width = DataGridObj.rows[1].cells[iCntr].clientWidth;
        }
        if (iCntr <= DataGridObj.rows[0].cells.length - 2) {
            spanTag.style.width = width + 'px';
        }
        else {
            spanTag.style.width = width + 20 + 'px';
        }
        DataGridObj.rows[1].cells[iCntr].style.width = width + 'px';
    }
    var tableWidth = DataGridObj.clientWidth;

    //********* Hidding the original header of GridView *******
    DataGridObj.rows[0].style.display = 'none';

    //********* Setting the same width of all the components **********
    HeaderDivObj.style.width = DataDivWidth + 'px';
    DataDivObj.style.width = DataDivWidth + 'px';
    DataGridObj.style.width = tableWidth + 'px';
    HeadertableObj.style.width = tableWidth + 20 + 'px';
    return false;
}



function CreateGridHeader_POValues(DataDiv, GridView1, HeaderDiv) {
    var DataDivObj = document.getElementById(DataDiv);
    var DataGridObj = document.getElementById(GridView1);
    var HeaderDivObj = document.getElementById(HeaderDiv);

    //********* Creating new table which contains the header row ***********
    //var HeadertableObj = HeaderDivObj.appendChild(document.createElement('table'));
    var HeadertableObj = document.getElementById("TableHeader_POValues");
    DataDivObj.style.paddingTop = '0px';
    var DataDivWidth = DataDivObj.clientWidth;
    DataDivObj.style.width = '5500px';

    //********** Setting the style of Header Div as per the Data Div ************
    HeaderDivObj.className = DataDivObj.className;
    HeaderDivObj.style.cssText = DataDivObj.style.cssText;

    //**** Making the Header Div scrollable. *****
    HeaderDivObj.style.overflow = 'auto';

    //*** Hiding the horizontal scroll bar of Header Div ****
    //*** this is because we have to scroll the Div along with the DataDiv.
    HeaderDivObj.style.overflowX = 'hidden';

    //**** Hiding the vertical scroll bar of Header Div ****
    HeaderDivObj.style.overflowY = 'hidden';
    HeaderDivObj.style.height = DataGridObj.rows[0].clientHeight + 'px';

    //**** Removing any border between Header Div and Data Div ****
    HeaderDivObj.style.borderBottomWidth = '0px';

    //********** Setting the style of Header Table as per the GridView ************
    HeadertableObj.className = DataGridObj.className;

    //**** Setting the Headertable css text as per the GridView css text
    HeadertableObj.style.cssText = DataGridObj.style.cssText;
    HeadertableObj.border = '1px';
    HeadertableObj.rules = 'all';
    HeadertableObj.cellPadding = DataGridObj.cellPadding;
    HeadertableObj.cellSpacing = DataGridObj.cellSpacing;

    //********** Creating the new header row **********
    var Row = HeadertableObj.insertRow(0);
    Row.className = DataGridObj.rows[0].className;
    Row.style.cssText = DataGridObj.rows[0].style.cssText;
    Row.style.fontWeight = 'bold';

    //******** This loop will create each header cell *********
    for (var iCntr = 0; iCntr < DataGridObj.rows[0].cells.length; iCntr++) {
        var spanTag = Row.appendChild(document.createElement('td'));
        spanTag.innerHTML = DataGridObj.rows[0].cells[iCntr].innerHTML;
        var width = 0;

        //****** Setting the width of Header Cell **********
        if (spanTag.clientWidth > DataGridObj.rows[1].cells[iCntr].clientWidth) {
            width = spanTag.clientWidth;
        }
        else {
            width = DataGridObj.rows[1].cells[iCntr].clientWidth;
        }
        if (iCntr <= DataGridObj.rows[0].cells.length - 2) {
            spanTag.style.width = width + 'px';
        }
        else {
            spanTag.style.width = width + 20 + 'px';
        }
        DataGridObj.rows[1].cells[iCntr].style.width = width + 'px';
    }
    var tableWidth = DataGridObj.clientWidth;

    //********* Hidding the original header of GridView *******
    DataGridObj.rows[0].style.display = 'none';

    //********* Setting the same width of all the components **********
    HeaderDivObj.style.width = DataDivWidth + 'px';
    DataDivObj.style.width = DataDivWidth + 'px';
    DataGridObj.style.width = tableWidth + 'px';
    HeadertableObj.style.width = tableWidth + 20 + 'px';
    return false;
}

