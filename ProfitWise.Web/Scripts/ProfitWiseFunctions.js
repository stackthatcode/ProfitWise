
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


// popOverSelector == .popover-marker

/** Must call this function on KO afterRender **/

ProfitWiseFunctions.popOverBehaviorInit = function (popOverSelector) {
    var hideAllPopovers = function (callersPopOver) {
        $(popOverSelector).each(function () {
            if (this != callersPopOver) {
                $(this).popover('hide');
            }
        });
    };

    $(popOverSelector)
        .on('click', function (e) {
            // if any other popovers are visible, hide them
            hideAllPopovers(this);

            $(this).popover('show');

            // handle clicking on the popover itself
            $('.popover').off('click').on('click', function (e) {
                e.stopPropagation(); // prevent event for bubbling up => will not get caught with document.onclick
            });

            e.stopPropagation();
        });

    $(document).on('click', function (e) {
        hideAllPopovers();
    });
};



ProfitWiseFunctions.launchBulkEditPopUp = function () {
    ShopifyApp.Modal.open({
        src: '/ProfitWise/Static/SimpleCogsEditor.html',
        title: 'Bulk Edit all Variant CoGS',
        width: 'small',
        height: 390,
        buttons: {
            primary:
                { label: "OK", callback: function(label) { ShopifyApp.Modal.close("OK"); } },
            secondary: [
                { label: "Cancel", callback: function(label) { ShopifyApp.Modal.close("Cancel"); } }
            ]
        }
    },
    function(result, data) {
        // alert("result: " + result + "   data: " + data);
    });
    }

