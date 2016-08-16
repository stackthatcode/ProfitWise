
// apiKey: '50d69dbaf54ee35929a946790d5884e4',
// shopOrigin: 'https://3duniverse.myshopify.com'

var shopifyInitialization = function (apiKey, shopOrigin, title) {

    ShopifyApp.init({
        apiKey: apiKey,
        shopOrigin: shopOrigin,
    });

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

