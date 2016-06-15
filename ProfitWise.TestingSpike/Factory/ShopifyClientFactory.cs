﻿using System;
using System.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Shopify.HttpClient;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Batch.Factory
{
    public interface IShopifyClientFactory
    {
        IShopifyHttpClient Make(string userId);
    }


    public class ShopifyClientFactory : IShopifyClientFactory
    {
        public IShopifyHttpClient Make(string userId)
        {            
            var context = ApplicationDbContext.Create();
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var shopifyCredentialService = new ShopifyCredentialService(userManager);
            var shopifyFromClaims = shopifyCredentialService.Retrieve(userId);

            // TODO - notify the Alerting Service on this type of failure

            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(shopifyFromClaims.Message);
            }

            var httpClient = new HttpClient();
            var shopifyClient =
                new ShopifyHttpClient(httpClient, shopifyFromClaims.ShopDomain, shopifyFromClaims.AccessToken);

            if (ConfigurationManager.AppSettings["ShopifyRetryLimit"] != null)
                shopifyClient.ShopifyRetryLimit = Int32.Parse(ConfigurationManager.AppSettings["ShopifyRetryLimit"]);

            if (ConfigurationManager.AppSettings["ShopifyTimeout"] != null)
                shopifyClient.ShopifyRetryLimit = Int32.Parse(ConfigurationManager.AppSettings["ShopifyTimeout"]);

            return shopifyClient;
        }
    }
}
