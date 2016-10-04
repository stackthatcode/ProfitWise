﻿var ProfitWiseShopify = ProfitWiseShopify || {};

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
                                label: "Reports",
                                href: ProfitWiseConfig.BaseUrl + "/UserMain/Reports",
                                target: "app"
                            },
                            {
                                label: "Edit Preferences",
                                href: ProfitWiseConfig.BaseUrl + "/UserMain/Preferences",
                                target: "app"
                            },
                            {
                                label: "Edit Product CoGS (legacy)",
                                href: "/ProfitWise/Static/CoGS.html",
                                target: "app"
                            },
                            {
                                label: "Edit Product CoGS",
                                href: ProfitWiseConfig.BaseUrl + "/UserMain/EditProductCogs",
                                target: "app"
                            },
                            {
                                label: "Manage Purchase Orders",
                                href: ProfitWiseConfig.BaseUrl + "/UserMain/ManagePurchaseOrders",
                                target: "app"
                            },
                            {
                                label: "Edit Goals",
                                href: ProfitWiseConfig.BaseUrl + "/UserMain/Goals",
                                target: "app"
                            },
                            {
                                label: "Product Consolidation",
                                href: ProfitWiseConfig.BaseUrl + "/UserMain/Products",
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
                                href: ProfitWiseConfig.BaseUrl + "/NonsenseUrl",
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
            ajax.HttpGet("/UserMain/Ping", this);
        },
        function () {            
            ShopifyApp.Modal.open(
                settings,
                function (result) {
                    //console.log(result);

                    if (result === true) {
                        callback();
                    }
                    if (result === "error") {
                        ProfitWiseShopify.ErrorPopup();
                    }
                });
        }
    );
};

ProfitWiseShopify.ErrorPopup = function () {
    ShopifyApp.Modal.alert({
        title: "System Error",
        message: "We're sorry for the inconvenience, but the System has encountered an error. " +
                "We'll reload the page. If the problem persists, reach out to our Support Team!",
        okButton: "Ok, Thanks"
    }, function (result) {
        window.location.reload();
    });
};



ProfitWiseShopify.LaunchBulkEditPopUp = function (masterProductId, callback) {
    var url = ProfitWiseConfig.BaseUrl + '/UserMain/BulkEditCogs?masterProductId=' + masterProductId;
    ProfitWiseShopify.LaunchModal({
        src: url,
        title: 'Bulk Edit all Variant CoGS',
        width: 'small',
        height: 380,
    }, callback);
};

ProfitWiseShopify.LaunchStockedDirectlyProductsPopup = function (callback) {
    var url = ProfitWiseConfig.BaseUrl + '/UserMain/StockedDirectlyProductsPopup';

    ProfitWiseShopify.LaunchModal({
            src: url,
            title: 'Set Stocked Directly for all Products in Search',
            width: 'small',
            height: 300,
        }, callback);
};



ProfitWiseShopify.LaunchStockedDirectlyVariantsPopup = function (shopifyProductId, callbackFunction) {
    var url = ProfitWiseConfig.BaseUrl + '/UserMain/StockedDirectlyVariantsPopup?shopifyProductId=' + shopifyProductId;

    ShopifyApp.Modal.open({
        src: url,
        title: 'Set Stocked Directly for all Product Variants',
        width: 'small',
        height: 200,
    },
        function (result) {
            if (result) {
                callbackFunction(shopifyProductId);
            }
        });
};

ProfitWiseShopify.LaunchExcludedProductVariantPopup = function(shopifyProductId, callbackFunction) {
    var url = ProfitWiseConfig.BaseUrl + '/UserMain/ExcludedVariantsPopup?shopifyProductId=' + shopifyProductId;

    ShopifyApp.Modal.open({
        src: url,
        title: 'Set Exclude/Include for all Product Variants',
        width: 'small',
        height: 200,
    },
        function (result) {
            if (result) {
                callbackFunction(shopifyProductId);
            }
        });

};

ProfitWiseShopify.LaunchExcludedProductsPopup = function (callbackFunction) {
    var url = ProfitWiseConfig.BaseUrl + '/UserMain/ExcludedProductsPopup';
    
    ShopifyApp.Modal.open({
        src: url,
        title: 'Set Exclude/Include for all Products in Search',
        width: 'small',
        height: 270,
    },
        function (result) {
            if (result) {
                callbackFunction();
            }
        });

};
