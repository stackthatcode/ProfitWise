
var ProfitWiseFunctions = ProfitWiseFunctions || {};


// movingElementSelector == '#top-header'

ProfitWiseFunctions.fixedHeaderScrollingInit = function (movingElementSelector) {
    $(window)
        .scroll(function() {
            $(movingElementSelector).css('left', -($(this).scrollLeft()) + "px");
        });
}


// var bodyColumnCells = $("table#cogs tbody tr td");
// var headerColumnHeaders = $("table#heading thead tr th");

ProfitWiseFunctions.tableHeaderWidthInit = function (bodyCellSelector, headerCellSelector) {
    var columnFixed = function () {
        var bodyColumnCells = $(bodyCellSelector);
        var headerColumnHeaders = $(headerCellSelector);

        for (var i = 0; i < bodyColumnCells.length; i++) {
            var widthCSS = $(bodyColumnCells[i]).css("width");
            $(headerColumnHeaders[i]).css("width", widthCSS);
        }
    };

    $(window)
        .resize(function () {
            columnFixed();
        });
    $(document).ready(function () {
        columnFixed();
    });
}

