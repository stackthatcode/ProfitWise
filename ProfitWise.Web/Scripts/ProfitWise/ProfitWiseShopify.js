var ProfitWiseShopify = ProfitWiseShopify || {};

ProfitWiseShopify.AppInitialize = function(apiKey, shopOrigin) {
    ShopifyApp.init({
        apiKey: apiKey,
        shopOrigin: shopOrigin,
    });
};

ProfitWiseShopify.BarInitialize = function (title) {
    var querystring = (location.search);
    
    var linkBuilder = function(path, label) {
        return {
            label: label,
            href: ProfitWiseConfig.BaseUrl + path + querystring,
            target: "app",
        }
    };

    ShopifyApp.ready(function () {
        ShopifyApp.Bar.initialize({
            title: title,

            buttons: {
                secondary: [
                    linkBuilder("", "Home"),
                    {
                        label: "Navigate to...",
                        type: "dropdown",
                        links: [
                            linkBuilder("", "Profitability Report"),
                            linkBuilder("/Report/GoodsOnHand", "Inventory Valuation Report"),
                            linkBuilder("/Cogs/Products", "Manage Products and CoGS"),
                            linkBuilder("/Preferences/Edit", "Edit Preferences"),
                            //linkBuilder("/Error/ThrowAnonymousError", "Error Page - Anon (Remove for Prod)"),
                        ]
                    },
                    {
                        label: "Support",
                        type: "dropdown",
                        links: [
                            linkBuilder("/Content/About", "About ProfitWise"),
                            linkBuilder("/Content/Contact", "Contact Us"),
                        ]
                    },
                ]
            }
        });

        ShopifyApp.Bar.loadingOff();
    });
};

// Parameters for the ShopifyApp.Modal.close() method
// { result: true } => invokes callback
// { result: false } => does nothing
// { result: error } => shows the Error Popup
ProfitWiseShopify.LaunchModal = function(settings, callback) {
    flow.exec(
        function () {
            var ajax = new ProfitWiseFunctions.Ajax();
            ajax.HttpGet("/Report/Ping", this);
        },
        function () {            
            ShopifyApp.Modal.open(
                settings,
                function (data) {
                    if (data.result === true) {
                        if (callback) {
                            callback(data);
                        }
                    }
                    if (data.result === "error") {
                        ProfitWiseShopify.ErrorPopup();
                    }
                });
        }
    );
};

ProfitWiseShopify.ErrorMessage =
    "We're sorry for the inconvenience, but the System has encountered an error. " +
    "Please try reloading the page. If the problem persists, reach out to our Support Team!";

ProfitWiseShopify.ErrorPopup = function () {
    ShopifyApp.Modal.alert({
        title: "System Error",
        message: ProfitWiseShopify.ErrorMessage,
    }, function (result) {
        //window.location.reload();
    });
};

