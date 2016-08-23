
var ProfitWiseShopify = ProfitWiseShopify || {};
// apiKey: '50d69dbaf54ee35929a946790d5884e4',
// shopOrigin: 'https://3duniverse.myshopify.com'

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
                        href: "/ProfitWise/",
                        target: "app",
                    },
                    {
                        label: "Navigate to...",
                        type: "dropdown",
                        links: [
                            {
                                label: "Dashboard",
                                href: "/ProfitWise/",
                                target: "app"
                            },
                            {
                                label: "Reports",
                                href: "/ProfitWise/UserMain/Reports",
                                target: "app"
                            },
                            {
                                label: "Edit Preferences",
                                href: "/ProfitWise/UserMain/Preferences",
                                target: "app"
                            },
                            {
                                label: "Edit Product CoGS (legacy)",
                                href: "/ProfitWise/Static/CoGS.html",
                                target: "app"
                            },
                            {
                                label: "Edit Product CoGS",
                                href: "/ProfitWise/UserMain/EditProductCogs",
                                target: "app"
                            },
                            {
                                label: "Manage Purchase Orders",
                                href: "/ProfitWise/UserMain/ManagePurchaseOrders",
                                target: "app"
                            },
                            {
                                label: "Edit Goals",
                                href: "/ProfitWise/UserMain/Goals",
                                target: "app"
                            },
                            {
                                label: "Product Consolidation",
                                href: "/ProfitWise/UserMain/Products",
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
                                href: "/ProfitWise/UserAuxiliary/About",
                                target: "app"
                            },
                            {
                                label: "Contact Us",
                                href: "/ProfitWise/UserAuxiliary/Contact",
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


ProfitWiseShopify.LaunchBulkEditPopUp = function(shopifyProductId, callbackFunction) {
    var url = '/ProfitWise/UserMain/BulkEditProductVariantCogs?shopifyProductId=' + shopifyProductId;

    ShopifyApp.Modal.open({
            src: url,
            title: 'Bulk Edit all Variant CoGS',
            width: 'small',
            height: 390,
        },
        function (result) {
            if (result) {
                callbackFunction(shopifyProductId);
            }
        });
};


ProfitWiseShopify.LaunchStockedDirectlyVariantsPopup = function (shopifyProductId, callbackFunction) {
    var url = '/ProfitWise/UserMain/StockedDirectlyVariantsPopup?shopifyProductId=' + shopifyProductId;

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

ProfitWiseShopify.LaunchStockedDirectlyProductsPopup = function (callbackFunction) {
    var url = '/ProfitWise/UserMain/StockedDirectlyProductsPopup';
    
    ShopifyApp.Modal.open({
        src: url,
        title: 'Set Stocked Directly for all Products in Search',
        width: 'small',
        height: 240,
    },
        function (result) {
            if (result) {
                callbackFunction();
            }
        });
};


ProfitWiseShopify.LaunchExcludedProductVariantPopup = function(shopifyProductId, callbackFunction) {
    var url = '/ProfitWise/UserMain/ExcludedVariantsPopup?shopifyProductId=' + shopifyProductId;

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
    var url = '/ProfitWise/UserMain/ExcludedProductsPopup';
    
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

