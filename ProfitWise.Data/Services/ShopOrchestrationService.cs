﻿using System;
using System.Configuration;
using System.Data;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model.Billing;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace ProfitWise.Data.Services
{
    public class ShopOrchestrationService
    {
        private readonly CurrencyService _currencyService;
        private readonly ShopRepository _shopRepository;
        private readonly MultitenantFactory _factory;
        private readonly ApiRepositoryFactory _apifactory;
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly HangFireService _hangFireService;
        private readonly IShopifyCredentialService _credentialService;
        private readonly TimeZoneTranslator _timeZoneTranslator;
        private readonly IPushLogger _logger;


        private static readonly int _orderStartOffsetMonths = 
                ConfigurationManager.AppSettings.GetAndTryParseAsInt("InitialOrderStartDateOffsetMonths", 3);

        private static readonly bool TestRecurringCharges =
                ConfigurationManager.AppSettings.GetAndTryParseAsBool("TestRecurringCharges", false);

        private static readonly string
                WebhookAddress = ConfigurationManager
                    .AppSettings.GetAndTryParseAsString("UninstallWebHookAddress", "");

        private const int DefaultFreeTrial = 14;
        private const decimal ProfitWiseMonthlyPrice = 14.95m;


        public ShopOrchestrationService(
                    CurrencyService currencyService, 
                    ShopRepository shopRepository, 
                    MultitenantFactory factory,
                    ConnectionWrapper connectionWrapper, 
                    ApiRepositoryFactory apifactory, 
                    HangFireService hangFireService, 
                    IShopifyCredentialService credentialService,
                    TimeZoneTranslator timeZoneTranslator,
                    IPushLogger logger)
        {
            _currencyService = currencyService;
            _shopRepository = shopRepository;
            _factory = factory;
            _logger = logger;
            _connectionWrapper = connectionWrapper;
            _apifactory = apifactory;
            _hangFireService = hangFireService;
            _credentialService = credentialService;
            _timeZoneTranslator = timeZoneTranslator;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }
        

        // ProfitWise Shop database record methods
        public PwShop CreateShop(string shopOwnerUserId, Shop shop, ShopifyCredentials credentials)
        {
            // Create the Shop record in SQL
            var currencyId = _currencyService.AbbreviationToCurrencyId(shop.Currency);

            var orderDatasetStartDate = DateTime.UtcNow.AddMonths(-Math.Abs(_orderStartOffsetMonths));
            
            var newShop = PwShop.Make(
                shopOwnerUserId, shop.Id, currencyId, shop.TimeZoneIana, shop.Domain, orderDatasetStartDate);

            newShop.PwShopId = _shopRepository.Insert(newShop);
            _logger.Info($"Created new Shop - UserId: {newShop.ShopOwnerUserId}");

            // Create the Batch State for Shop
            var profitWiseBatchStateRepository = _factory.MakeBatchStateRepository(newShop);
            var state = new PwBatchState
            {
                PwShopId = newShop.PwShopId,
            };

            profitWiseBatchStateRepository.Insert(state);
            _logger.Info($"Created Batch State for Shop - UserId: {newShop.ShopOwnerUserId}");
            
            return newShop;
        }

        public void UpdateShop(string userId, string currencySymbol, string timeZoneIana)
        {
            var pwShop = _shopRepository.RetrieveByUserId(userId);
            pwShop.CurrencyId = _currencyService.AbbreviationToCurrencyId(currencySymbol); ;
            pwShop.TimeZone = timeZoneIana;

            using (var transaction = _shopRepository.InitiateTransaction())
            {
                _shopRepository.Update(pwShop);
                _shopRepository.UpdateIsAccessTokenValid(pwShop.PwShopId, true);
                _shopRepository.UpdateIsProfitWiseInstalled(pwShop.PwShopId, true, null);
                transaction.Commit();
            }

            _logger.Debug($"Updated Shop - UserId: {pwShop.ShopOwnerUserId}");
        }


        // Webhook subscription & maintenance service
        public void UpsertUninstallWebhook(ShopifyCredentials credentials)
        {
            // Create the Webhook via Shopify API
            var apiRepository = _apifactory.MakeWebhookApiRepository(credentials);
            var shop = _shopRepository.RetrieveByUserId(credentials.ShopOwnerUserId);

            var existingWebhook = apiRepository.Retrieve(Webhook.UninstallTopic, WebhookAddress);
            if (existingWebhook != null)
            {
                var updateRequest = 
                    Webhook.MakeAddressUpdateRequest(existingWebhook.Id, WebhookAddress);
                apiRepository.UpdateAddress(updateRequest);
                return;
            }
            else
            {
                var request = Webhook.MakeUninstallHookRequest(WebhookAddress);
                var webhook = apiRepository.Subscribe(request);
                _shopRepository.UpdateShopifyUninstallId(credentials.ShopOwnerUserId, webhook.Id);
            }
        }


        // Recurring Application Charge methods
        public static RecurringApplicationCharge MakeCharge()
        {
            var charge = new RecurringApplicationCharge()
            {
                name = "ProfitWise Monthly Charge",
                trial_days = DefaultFreeTrial,
                price = ProfitWiseMonthlyPrice,
                test = TestRecurringCharges ? true : (bool?)null,
            };
            return charge;
        }

        public PwRecurringCharge CreateCharge(string userId, string returnUrl)
        {
            var credentials = _credentialService.Retrieve(userId).ToShopifyCredentials();
            var repository = _apifactory.MakeRecurringApiRepository(credentials);
            var shop = _shopRepository.RetrieveByUserId(userId);

            // Invoke Shopify API to create the Recurring Application Charge
            var chargeParameter = MakeCharge();
            chargeParameter.return_url = returnUrl;

            var billingRepository = _factory.MakeBillingRepository(shop);
            if (shop.TempFreeTrialOverride.HasValue)
            {
                // If a Free Trial Override has been set, then use that
                chargeParameter.trial_days = shop.TempFreeTrialOverride.Value;
            }
            else if (billingRepository.AnyHistory())
            {
                // If this has been a previous User, set the Trial to 0
                chargeParameter.trial_days = 0;
            }
            var apiChargeResult = repository.UpsertCharge(chargeParameter);

            // Write a record in ProfitWise's Recurring Charge table
            using (var transaction = billingRepository.InitiateTransaction())
            {
                _shopRepository.UpdateTempFreeTrialOverride(shop.PwShopId, null);

                var nextChargeId = billingRepository.RetrieveNextKey();
                var profitWiseCharge = new PwRecurringCharge
                {
                    PwShopId = shop.PwShopId,
                    PwChargeId = nextChargeId,
                    IsPrimary = true,
                    ShopifyRecurringChargeId = apiChargeResult.id,
                    ConfirmationUrl = apiChargeResult.confirmation_url,
                    LastStatus = apiChargeResult.status.ToChargeStatus(),
                    LastJson = apiChargeResult.SerializeToJson(),
                };
                billingRepository.Insert(profitWiseCharge);
                // Important: Update Primary to nuke the previous Charge
                billingRepository.UpdatePrimary(nextChargeId);
                transaction.Commit();
                return profitWiseCharge;
            }
        }

        public PwRecurringCharge SyncAndRetrieveCurrentCharge(string userId)
        {
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            var repository = _apifactory.MakeRecurringApiRepository(credentials);

            // Create Billing Repository and get the current primary Recurring Charge record
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _factory.MakeBillingRepository(shop);

            var currentCharge = billingRepository.RetrieveCurrent();
            if (currentCharge == null)
            {
                return null;
            }

            // Invoke Shopify API to get the Charge
            var result = repository.RetrieveCharge(currentCharge.ShopifyRecurringChargeId);
            if (result == null)
            {
                return null;
            }

            // Update ProfitWise's local database record
            currentCharge.ConfirmationUrl = result.confirmation_url;
            currentCharge.LastStatus = result.status.ToChargeStatus();
            currentCharge.LastJson = result.SerializeToJson();
            billingRepository.Update(currentCharge);

            return currentCharge;
        }

        public bool VerifyChargeAndScheduleRefresh(string userId)
        {
            // Synchronize the Charge record in ProfitWise with Shopify API
            SyncAndRetrieveCurrentCharge(userId);

            // The Shop should reflect the status
            var shop = _shopRepository.RetrieveByUserId(userId);
            if (shop.IsBillingValid)
            {
                // Protective measure to prevent multiple background updates
                _hangFireService.KillInitialRefresh(userId);
                _hangFireService.KillRoutineRefresh(userId);

                // ... and finally schedule an immediate update
                _hangFireService.AddOrUpdateInitialShopRefresh(userId);
                return true;
            }
            else
            {
                var billingRepository = _factory.MakeBillingRepository(shop);
                billingRepository.ClearPrimary();
                return false;
            }
        }

        public bool SyncAndValidateBilling(PwShop shop)
        {
            var charge = SyncAndRetrieveCurrentCharge(shop.ShopOwnerUserId);

            if (charge == null)
            {
                return false;
            }
            if (charge.LastStatus == ChargeStatus.Active)
            {
                return true;
            }
            if (charge.LastStatus == ChargeStatus.Accepted)
            {
                ActivateCharge(shop.ShopOwnerUserId, charge.PwChargeId);
                return true;
            }

            // Charge Status is neither Active nor Accepted, thus set Billing Valid to false
            return false;
        }

        public PwRecurringCharge ActivateCharge(string userId, long pwChargeId)
        {
            // Retrieve Credentials from Claims
            var shop = _shopRepository.RetrieveByUserId(userId);
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();

            // Get the Charge from the ProfitWise database
            var billingRepository = _factory.MakeBillingRepository(shop);
            var pwCharge = billingRepository.Retrieve(pwChargeId);

            // Invoke Shopify API to get the Charge, the Activate it
            var repository = _apifactory.MakeRecurringApiRepository(credentials);
            var existingCharge = repository.RetrieveCharge(pwCharge.ShopifyRecurringChargeId);
            var activtedCharge = repository.ActivateCharge(existingCharge);

            // Update the ProfitWise database record with Activated Charge data
            pwCharge.ConfirmationUrl = activtedCharge.confirmation_url;
            pwCharge.LastStatus = activtedCharge.status.ToChargeStatus();
            pwCharge.LastJson = activtedCharge.SerializeToJson();

            using (var transaction = billingRepository.InitiateTransaction())
            {
                billingRepository.Update(pwCharge);
                billingRepository.UpdatePrimary(pwCharge.PwChargeId);
                transaction.Commit();
            }

            return pwCharge;
        }

        public void CancelCharge(string userId, long pwChargeId)
        {
            // Get the Recurring Charge record fro ProfitWise
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _factory.MakeBillingRepository(shop);
            var charge = billingRepository.Retrieve(pwChargeId);
            if (charge == null)
            {
                return;
            }

            var credentials = _credentialService.Retrieve(userId).ToShopifyCredentials();
            var apiRepository = _apifactory.MakeRecurringApiRepository(credentials);

            // Cancel the Charge in Shopify
            try
            {
               apiRepository.CancelCharge(charge.ShopifyRecurringChargeId);
            }
            catch (Exception e)
            {
                // Swallow the exception!
                _logger.Error(e);
            }

            // Retrieve the Charge from Shopify API
            var result = apiRepository.RetrieveCharge(charge.ShopifyRecurringChargeId);
            if (result != null)
            {
                // If it exists, use its properties
                charge.LastStatus = result.status.ToChargeStatus();
                charge.LastJson = result.SerializeToJson();
                charge.IsPrimary = false;
            }
            else
            {
                // Can't get it from Shopify? Fine - set it to Cancelled anyway
                charge.LastStatus = ChargeStatus.Cancelled;
                charge.IsPrimary = false;
            }

            billingRepository.Update(charge);
        }


        // Uninstallation process
        public void UninstallShop(long shopifyShopId)
        {
            var shop = _shopRepository.RetrieveByShopifyShopId(shopifyShopId);
            _shopRepository.UpdateIsProfitWiseInstalled(shop.PwShopId, false, DateTime.UtcNow);
        }

        // Kills the Background Jobs from Refreshing and nukes the Billing (if it isn't already!)
        public void FinalizeUninstallation(int pwShopId)
        {
            var shop = _shopRepository.RetrieveByShopId(pwShopId);
            var billingRepository = _factory.MakeBillingRepository(shop);

            // Kill the Recurring Shop Refresh Job
            _hangFireService.KillRoutineRefresh(shop.ShopOwnerUserId);
            _hangFireService.KillInitialRefresh(shop.ShopOwnerUserId);

            // Clear out any ProfitWise Charge records to force creation of a new Charge
            billingRepository.ClearPrimary();
        }
    }
}

