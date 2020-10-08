// When page first loads, ensure that displayed results are filtered correctly
// based on whether the checkbox is checked.
document.body.onload = function () {
    OnToggleFilter();
};

// Called whenever the filter checkbox is toggled. Hides or shows
// tables on the page based on the state of the checkbox, and 
// whether or not the tables contain changed variables.
function OnToggleFilter() {
    var checkbox = document.getElementById("chkFilter");
    var show = !checkbox.checked; // bool - show elements, or hide them?

    // First, toggle visibility for all rows containing data
    // which has not changed.
    var rows = document.getElementsByClassName("nodiff");
    for (var i = 0; i < rows.length; i++) {
        toggleVisibility(rows[i], show)
    }

    // and each row containing a model name (e.g. Barley) has a class name of
    // SimulationName.

    // Next, hide all 'sub tables' representing a table of results 
    // (e.g.AustraliaHarvestObsPred), if the results are all hidden. These rows 
    // have a class name of TableName.
    hideElementsWithNoChildren("TableName", ["SimulationName", "TableName"], show);

    // Finally, hide all 'sub tables' representing a model, if the
    // 'sub-sub tables (TableName tables)' containing a table of results have 
    // all been hidden.
    hideElementsWithNoChildren("SimulationName", ["SimulationName"], show);
}

// Modifies the visibility of an element.
//
// @param element:              object:     The element whose visibility will be modified.
// @param show:                 boolean:    Should the element be visible?    
//
// @return none
function toggleVisibility(element, show) {
    element.style.display = show ? "" : "none";
}

// Checks if an element is hidden.
//
// @param element:              object:     The element whose visibility will be checked.
//
// @return                      bool:       True iff the element is visible.
function elementIsHidden(element) {
    return element.style.display == "none";
}

// Checks if all 'children' of an element is hidden.
// Technically, this is actually just checking whether subsequent rows in the table
// are hidden.
//
// @param element:              object:     The element whose children's visibility will be checked.
// @param childNamesToMatch:    string[]:   Array of class names. We will stop checking sibling nodes'
//                                          visibility once we reach an element with this class name.
//
// @return                      bool:       True iff all 'child' elements are visible.
function allChildrenAreHidden(element, childNamesToMatch) {
    var i;

    // First, get the index of this element in its array of siblings.
    var index = Array.prototype.indexOf.call(element.parentElement.children, element); // ugh!

    if (index < 0)
        return true; // This should never happen if `element` is a legitimate member of the DOM.

    // Next, iterate over siblings starting at the first sibling after `element`.
    // Stop when we reach a sibling whose class name is included in `childNamesToMatch`.
    for (i = index + 1; i < element.parentElement.children.length; i++) {
        var node = element.parentElement.children[i];
        if (childNamesToMatch.includes(node.className))
            return true;
        if (!elementIsHidden(node))
            return false;
    }
    return true;
}


// Hides or shows all elements of a given class, if all of their children are hidden.
//
// The page is actually made up of 1 table. Each 'sub table' is just made up of rows 
// in the master level table. Therefore, 'children' of a particular row are actually 
// sibling rows in the table.
//
// @param className:            string:     Class whose elements will be hidden if
//                                          all of their children are also hidden.
// @param childClassNames:      string[]:   List of class names of elements who are
//                                          not considered children of `className`.
//
// @return none
function hideElementsWithNoChildren(className, childClassNames, show) {
    rows = document.getElementsByClassName(className);
    for (var i = 0; i < rows.length; i++) {
        if (show)
            toggleVisibility(rows[i], show);
        else if (allChildrenAreHidden(rows[i], childClassNames))
            toggleVisibility(rows[i], false);
    }
}