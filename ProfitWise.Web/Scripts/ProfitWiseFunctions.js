
var ProfitWiseFunctions = ProfitWiseFunctions || {};


// movingElementSelector == '#top-header'

ProfitWiseFunctions.FixedHeaderScrollingInit = function (movingElementSelector) {
    $(window)
        .scroll(function() {
            $(movingElementSelector).css('left', -($(this).scrollLeft()) + "px");
        });
}


// var bodyColumnCells = $("table#cogs tbody tr td");
// var headerColumnHeaders = $("table#heading thead tr th");

ProfitWiseFunctions.TableHeaderWidthInit = function (bodyCellSelector, headerCellSelector) {
    var columnFixed = function () {
        var bodyColumnCells = $(bodyCellSelector);
        var headerColumnHeaders = $(headerCellSelector);

        for (var i = 0; i < bodyColumnCells.length; i++) {
            var widthCSS = $(bodyColumnCells[i]).css("width");
            $(headerColumnHeaders[i]).css("width", widthCSS);
        }
    };

    $(window).resize(function () {
        columnFixed();
    });
    $(document).ready(function () {
        columnFixed();
    });
}


// Relies upon .popover-container for enclosing parent & .popover-launcher for the triggering element
ProfitWiseFunctions.PopOverCloseAll = function() {
    $('div.popover:visible')
        .closest('.popover-container')
        .find(".popover-launcher")
        .popover("hide")
        .each(function (index, element) {
            $(element).data()["bs.popover"]["inState"]["click"] = false;
        });
};

ProfitWiseFunctions.PopOverAutoCloseInit = function () {
    $(document).on("click", function (event) {
        $('div.popover:visible')
            .closest('.popover-container')
            .not($(event.target).closest(".popover-container"))
            .find(".popover-launcher")
            .popover("hide")
            .each(function (index, element) {
                $(element).data()["bs.popover"]["inState"]["click"] = false;
            });
    });

/*    $(document)
        .on("scroll",
            function() {
                ProfitWiseFunctions.PopOverCloseAll();
            });*/
};


ProfitWiseFunctions.CurrencyCache = [
    { Id: 1, Abbr: "USD", Symbol: "$" },
    { Id: 2, Abbr: "EUR", Symbol: "€" },
    { Id: 3, Abbr: "JPY", Symbol: "¥" },
    { Id: 4, Abbr: "GBP", Symbol: "£" },
    { Id: 5, Abbr: "AUD", Symbol: "$" },
    { Id: 6, Abbr: "CHF", Symbol: "Fr" },
    { Id: 7, Abbr: "CAD", Symbol: "$" },
];


ProfitWiseFunctions.FormatCurrencyWithAbbr = function (amount, currencyId) {
    var item =
        AQ(ProfitWiseFunctions.CurrencyCache)
            .first(function (item) { return item.Id == currencyId; });

    return item.Symbol + numeral(amount).format("0,0.00") + " " + item.Abbr;
};

ProfitWiseFunctions.FormatCurrency = function (amount, currencyId) {
    var item =
        AQ(ProfitWiseFunctions.CurrencyCache)
            .first(function (item) { return item.Id == currencyId; });

    return item.Symbol + numeral(amount).format("0,0.00");
};



// Temporary hard-coding of reference to ProfitWiseShopify.js

ProfitWiseFunctions.AjaxSettings = {
    // Override these settings in the PushLibrary Shim
    BaseUrl: "/ProfitWise",
    Timeout: 60000,
    WaitingLayerSelector: "#spinner-layer",
    ErrorCallbackFunction: ProfitWiseShopify.ErrorPopup,
};


ProfitWiseFunctions.Ajax = function (settings) {
    var self = this;

    if (settings) {
        self.Settings = settings;
    } else {
        self.Settings = ProfitWiseFunctions.AjaxSettings;
    }


    self.ErrorCallback = function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status != 0 || textStatus == "timeout") {
            self.HideLoading();
            self.Settings.ErrorCallbackFunction();
        }
    };

    self.HttpGet = function (url, successFunc) {
        flow.exec(
            function () {
                self.ShowLoading();
                $.ajax({
                    type: 'GET',
                    url: self.Settings.BaseUrl + url,
                    timeout: self.Settings.Timeout,
                    error: self.ErrorCallback,
                    success: this
                });
            },
            function (data, textStatus, jqXHR) {
                self.HideLoading();
                if (successFunc) {
                    successFunc(data, textStatus, jqXHR);
                }
            }
        );
    };

    self.HttpPost = function (url, data, successFunc) {
        flow.exec(
            function () {
                self.ShowLoading();
                $.ajax({
                    type: 'POST',
                    dataType: 'json',
                    contentType: 'application/json; charset=utf-8',
                    url: self.Settings.BaseUrl + url,
                    data: JSON.stringify(data),
                    timeout: self.Settings.Timeout,
                    error: self.ErrorCallback,
                    success: this
                });
            },
            function (data, textStatus, jqXHR) {
                self.HideLoading();
                if (successFunc) {
                    successFunc(data, textStatus, jqXHR);
                }
            }
        );
    };

    self.ShowLoading = function () {
        $(self.Settings.WaitingLayerSelector).show();
    };

    self.HideLoading = function () {
        $(self.Settings.WaitingLayerSelector).hide();
    };
};
