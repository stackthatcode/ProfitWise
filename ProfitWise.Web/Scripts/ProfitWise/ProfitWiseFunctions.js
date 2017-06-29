
var ProfitWiseFunctions = ProfitWiseFunctions || {};


String.prototype.trunc = function (n) {
    return (this.length > n) ? this.substr(0, n - 1) + '...' : this;
};

String.prototype.parseToJavascriptDate =
    function (params) {
        if (params && params.parseTimeZone) {
            return moment(this).toDate();
        } else {
            return moment(this.substring(0, 10)).toDate();
        }
    };

ProfitWiseFunctions.parseToJavascriptDate =
    function (input) {
        return moment(input).toDate();
    };


ProfitWiseFunctions.ToDecimalPlaces =
    function (input) {
        return parseFloat(Math.round(input * 100) / 100).toFixed(2);
    };

ProfitWiseFunctions.MomentStartOfDay =
    function (momentObject) {
        return momentObject.hours(0).minutes(0).seconds(0).milliseconds(0);
    };


ProfitWiseFunctions.getRandomInt = function(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min)) + min;
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


// Move this to the Tour Partial View
ProfitWiseFunctions.TourFactory = function (steps) {
    var tour = new Shepherd.Tour({
        defaults: {
            classes: 'shepherd-theme-arrows',
        }
    });

    var counter = 1;
    var numberOfSteps = steps.length;

    AQ(steps).each(
        function (step) {            
            var tourStepOptions = {
                text: step.content,
                title: step.title,
            };

            if (step.element) {
                if (step.placement) {
                    tourStepOptions.attachTo = step.element + " " + step.placement
                } else {
                    tourStepOptions.attachTo = step.element;
                }
            }

            var exitButton = { text: 'Exit', action: tour.cancel, classes: 'shepherd-exit-button' };
            var backButton = { text: 'Back', action: tour.back };
            var nextButton = { text: 'Next', action: tour.next };

            tourStepOptions.buttons = [ exitButton ];
            if (counter != 1) {
                tourStepOptions.buttons.push(backButton);
            }            
            if (counter != numberOfSteps) {
                tourStepOptions.buttons.push(nextButton);
            }

            tourStepOptions.when = {
                show: function (context) {
                    if (step.element) {
                        $(".shepherd-tour-bg-light").show();
                        $('html, body').animate({ scrollTop: $(step.element).offset().top }, 250);
                    } else {
                        $(".shepherd-tour-bg-dark").show();
                    }
                },
                hide: function () {
                    $(".shepherd-tour-bg").hide();
                },
            };

            var tourStepName = "tour-step-" + counter++;

            tour.addStep(tourStepName, tourStepOptions);
        });

    return tour;
};




ProfitWiseFunctions.ShowTour = function (tourIdentifier) {
    flow.exec(
		function() {
		    var settings = new ProfitWiseFunctions.AjaxSettings();
		    settings.UseSpinner = false;
		    var ajax = new ProfitWiseFunctions.Ajax(settings);
		    ajax.HttpPost("/Content/ShowTour?tourIdentifier=" + tourIdentifier, {}, this);
		},
        function (response) {
			// Do we need to do anything...?
		});
};


// Temporary hard-coding of reference to ProfitWiseShopify.js
ProfitWiseFunctions.AjaxSettings = function (modal) {
    var errorCallback;
    if (modal) {
        errorCallback = function () {
            alert(ProfitWiseShopify.ErrorMessage);
            //location.reload();
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
        UseSpinner: true,
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

    self.AjaxUniqueUrl = function(url) {
        var dateStampParam = "_=" + new Date().getTime();
        return url.indexOf("?") == -1 ? url + "?" + dateStampParam : url + "&" + dateStampParam;
    };

    self.HttpGet = function (url, successFunc) {
        flow.exec(
            function () {
                self.ShowLoading();
                $.ajax({
                    type: 'GET',
                    url: self.Settings.BaseUrl + self.AjaxUniqueUrl(url),
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

    self.HttpPost = function (url, data, successFunc, token) {
        flow.exec(
            function () {
                self.ShowLoading();

                $.ajax({
                    type: 'POST',
                    dataType: 'json',
                    beforeSend: function (request) {
                        request.setRequestHeader("__RequestVerificationToken", token);
                    },
                    contentType: 'application/json; charset=utf-8',
                    url: self.Settings.BaseUrl + self.AjaxUniqueUrl(url),
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
        if (self.Settings.UseSpinner) {
            $(self.Settings.WaitingLayerSelector).show();
        }
    };

    self.HideLoading = function () {
        if (self.Settings.UseSpinner) {
            $(self.Settings.WaitingLayerSelector).hide();
        }
    };
};

