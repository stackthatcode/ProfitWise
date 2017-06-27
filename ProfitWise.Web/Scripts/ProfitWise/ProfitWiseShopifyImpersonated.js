var ProfitWiseShopify = ProfitWiseShopify || {};
var ProfitWiseConfig = ProfitWiseConfig || {};

ProfitWiseShopify.LaunchModal = function (settings, callback) {
    $('.modal').modal({ show: true });
    $('.modal').on('shown.bs.modal', function () {
        //correct here use 'shown.bs.modal' event which comes in bootstrap3
        $(this).find('iframe').attr('src', settings.src);
    });

    $('.modal').on('hidden.bs.modal',
        function() {
            callback();
        });

    var eventMethod = window.addEventListener ? "addEventListener" : "attachEvent";
    var eventer = window[eventMethod];
    var messageEvent = eventMethod == "attachEvent" ? "onmessage" : "message";

    // Listen to message from child window
    eventer(messageEvent, function (e) {
        // Close the dialog box
        $('.modal').modal('hide');

        console.log(callback);

        // Invoke the callback with data!        
        if (e.data && e.data.result === true) {
            if (callback) {
                callback(e.data);
            }
        }
        if (e.data.result === "error") {
            ProfitWiseShopify.ErrorPopup();
        }
    }, false);
}

ProfitWiseShopify.CloseModal = function (data) {
    console.log(ProfitWiseConfig.BaseUrl);
    parent.postMessage(data, ProfitWiseConfig.BaseUrl);
};

ProfitWiseShopify.Confirm = function (settings) {
    var result = confirm(settings.message);
    if (result && settings.callback) {
        settings.callback(true);
    }
};

ProfitWiseShopify.Alert = function (settings) {
    alert(settings.message);
};

ProfitWiseShopify.ErrorMessage =
    "We're sorry for the inconvenience, but the System has encountered an error. " +
    "Please try reloading the page. If the problem persists, reach out to our Support Team!";

ProfitWiseShopify.ErrorPopup = function () {
    alert(ProfitWiseShopify.ErrorMessage);
};

