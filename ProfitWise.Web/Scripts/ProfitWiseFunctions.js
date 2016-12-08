﻿
var ProfitWiseConfig = ProfitWiseConfig || {};
ProfitWiseConfig.BaseUrl = '/ProfitWise';

var ProfitWiseFunctions = ProfitWiseFunctions || {};

ProfitWiseFunctions.FixedHeaderScrollingInit = function (movingElementSelector) {
    $(window)
        .scroll(function() {
            $(movingElementSelector).css('left', -($(this).scrollLeft()) + "px");
        });
}

ProfitWiseFunctions.SynchronizeWidth = function (sourceSelector, targetSelector) {
    var columnFixed = function () {
        var sourceCells = $(sourceSelector);
        var targetCells = $(targetSelector);

        for (var i = 0; i < sourceCells.length; i++) {
            var widthCSS = $(sourceCells[i]).css("width");
            $(targetCells[i]).css("width", widthCSS);
        }
    };

    $(window).resize(function () {
        columnFixed();
    });
    $(document).ready(function () {
        columnFixed();
    });

    columnFixed();
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
};

ProfitWiseFunctions.BsActivateTab = function(tab) {
    $('.nav-tabs a[href="#' + tab + '"]').tab('show');
};

ProfitWiseFunctions.CaseInsensitiveContains = function (input, substring) {
    var adjustedInput = (input || "").toUpperCase();
    var adjustedSubstring = (substring || "").toUpperCase();

    return adjustedInput.indexOf(adjustedSubstring) != -1;
};


// Temporary hard-coding of reference to ProfitWiseShopify.js
ProfitWiseFunctions.AjaxSettings = function (modal) {
    var errorCallback;
    if (modal) {
        errorCallback = function () {
            alert(ProfitWiseShopify.ErrorMessage);
            window.reload();
        }
    } else {
        errorCallback = function () {
            ProfitWiseShopify.ErrorPopup();
        };
    }

    return {
        BaseUrl: ProfitWiseConfig.BaseUrl,
        Timeout: 60000,
        WaitingLayerSelector: "#spinner-layer",
        ErrorCallbackFunction: errorCallback,
    };
};

ProfitWiseFunctions.Ajax = function (settings) {
    var self = this;
    self.Settings = settings || new ProfitWiseFunctions.AjaxSettings();

    self.ErrorCallback = function (jqXHR, textStatus, errorThrown) {
        // TODO: decide what to do here...
        //console.log(errorThrown);

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
