var ProfitWiseShopify = ProfitWiseShopify || {};

var ProfitWiseConfig = ProfitWiseConfig || {};
ProfitWiseConfig.BaseUrl = '/ProfitWise';

ProfitWiseShopify.AppInitialize = function(apiKey, shopOrigin) {
    ShopifyApp.init({
        apiKey: apiKey,
        shopOrigin: shopOrigin,
    });
};

ProfitWiseShopify.BarInitialize = function (title) {

    ShopifyApp.ready(function () {
        ShopifyApp.Bar.initialize({
            title: title,

            buttons: {
                secondary: [
                    {
                        label: "Home",
                        href: ProfitWiseConfig.BaseUrl,
                        target: "app",
                    },
                    {
                        label: "Navigate to...",
                        type: "dropdown",
                        links: [
                            {
                                label: "Dashboard",
                                href: ProfitWiseConfig.BaseUrl,
                                target: "app"
                            },
                            {
                                label: "Edit Preferences",
                                href: ProfitWiseConfig.BaseUrl + "/Preferences/Edit",
                                target: "app"
                            },
                            {
                                label: "Edit Product CoGS",
                                href: ProfitWiseConfig.BaseUrl + "/Cogs/Products",
                                target: "app"
                            },
                            {
                                label: "Edit Goals",
                                href: ProfitWiseConfig.BaseUrl + "/Report/Goals",
                                target: "app"
                            },
                            {
                                label: "Product Consolidation",
                                href: ProfitWiseConfig.BaseUrl + "/Report/Products",
                                target: "app"
                            },
                            {
                                label: "Error Page - Anon (Remove for Prod)",
                                href: ProfitWiseConfig.BaseUrl + "/Error/ThrowAnonymousError",
                                target: "app"
                            },
                            {
                                label: "Error Page - Authed (Remove for Prod)",
                                href: ProfitWiseConfig.BaseUrl + "/Error/ThrowAuthenticatedError",
                                target: "app"
                            },
                            {
                                label: "Not Found - 404 (Remove for Prod)",
                                href: ProfitWiseConfig.BaseUrl + "/Bullshit/NonsenseUrl",
                                target: "app"
                            },
                        ]
                    },
                    {
                        label: "Support",
                        type: "dropdown",
                        links: [
                            {
                                label: "About ProfitWise",
                                href: ProfitWiseConfig.BaseUrl + "/UserAuxiliary/About",
                                target: "app"
                            },
                            {
                                label: "Contact Us",
                                href: ProfitWiseConfig.BaseUrl + "/UserAuxiliary/Contact",
                                target: "app"
                            }
                        ]
                    },
                ]
            }
        });

        ShopifyApp.Bar.loadingOff();
    });
};

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

ProfitWiseShopify.ErrorMessage = "We're sorry for the inconvenience, but the System has encountered an error. " +
    "Please try reloading the page. If the problem persists, reach out to our Support Team!";

ProfitWiseShopify.ErrorPopup = function () {
    ShopifyApp.Modal.alert({
        title: "System Error",
        message: ProfitWiseShopify.ErrorMessage,
    }, function (result) {
        window.location.reload();
    });
};

