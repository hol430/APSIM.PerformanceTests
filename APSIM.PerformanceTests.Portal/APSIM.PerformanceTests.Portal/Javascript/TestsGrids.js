document.body.onload = function () {
    OnToggleFilter();
};

function OnToggleFilter() {
    var checkbox = document.getElementById("chkFilter");
    var show = !checkbox.checked; // bool - show elements, or hide them?

    // First, toggle visibility for all rows containing data
    // which has not changed.
    var rows = document.getElementsByClassName("nodiff");
    for (var i = 0; i < rows.length; i++) {
        toggleVisibility(rows[i], show)
    }

    // Set visibility for table name rows.
    hideElementsWithNoChildren("TableName", ["SimulationName", "TableName"], show);
    hideElementsWithNoChildren("SimulationName", ["SimulationName"], show);
}

function toggleVisibility(element, show) {
    element.style.display = show ? "" : "none";
}

function elementIsHidden(element) {
    return element.style.display == "none";
}

function allChildrenAreHidden(element, childNamesToMatch) {
    var i;

    // First, get the index of this element in its array of siblings.
    var index = Array.prototype.indexOf.call(element.parentElement.children, element); // ugh!

    if (index < 0)
        return true;

    for (i = index + 1; i < element.parentElement.children.length; i++) {
        var node = element.parentElement.children[i];
        if (childNamesToMatch.includes(node.className))
            return true;
        if (!elementIsHidden(node) && node.className != "blank")
            return false;
    }
    return true;
}

function hideElementsWithNoChildren(className, childClassNames, show) {
    rows = document.getElementsByClassName(className);
    for (var i = 0; i < rows.length; i++) {
        if (show)
            toggleVisibility(rows[i], show);
        else if (allChildrenAreHidden(rows[i], childClassNames))
            toggleVisibility(rows[i], false);
    }
}