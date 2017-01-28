using System;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Shopify;
using Push.Shopify.HttpClient;

namespace ProfitWise.TestData.TestDataBuilder
{
    public class OrderMakerProcess
    {
        private readonly IShopifyCredentialService _shopifyCredentialService;

        public OrderMakerProcess(IShopifyCredentialService shopifyCredentialService)
        {
            _shopifyCredentialService = shopifyCredentialService;
        }

        public void Execute(string userId)
        {
            var shopifyFromClaims = _shopifyCredentialService.Retrieve(userId);
            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(
                    $"ShopifyCredentialService unable to Retrieve for User {userId}: {shopifyFromClaims.Message}");
            }

            var shopifyClientCredentials = new ShopifyCredentials()
            {
                ShopOwnerUserId = shopifyFromClaims.ShopOwnerUserId,
                ShopDomain = shopifyFromClaims.ShopDomain,
                AccessToken = shopifyFromClaims.AccessToken,
            };


        }

        // Keep this in mind
        //https://ecommerce.shopify.com/c/shopify-apis-and-technology/t/remove-limit-of-orders-for-dev-shop-213408

    }
}

