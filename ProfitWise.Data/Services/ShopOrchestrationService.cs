using System;
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
        private readonly IPushLogger _logger;


        private readonly int _orderStartOffsetMonths = 
                ConfigurationManager.AppSettings.GetAndTryParseAsInt("InitialOrderStartDateOffsetMonths", 3);

        private static readonly bool TestRecurringCharges =
                ConfigurationManager.AppSettings.GetAndTryParseAsBool("TestRecurringCharges", false);

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
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }
        

        // ProfitWise Shop dawtabase record methods
        public PwShop CreateShop(string shopOwnerUserId, Shop shop, ShopifyCredentials credentials)
        {
            // Create the Shop record in SQL
            var currencyId = _currencyService.AbbreviationToCurrencyId(shop.Currency);
            var newShop = PwShop.Make(
                shopOwnerUserId, shop.Id, currencyId, shop.TimeZone, shop.Domain, _orderStartOffsetMonths);
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

            // Create the Webhook via Shopify API
            var apiRepository = _apifactory.MakeWebhookApiRepository(credentials);            
            var request = Webhook.MakeUninstallHookRequest();
            var webhook = apiRepository.Subscribe(request);

            // Store the Webhook Id 
            _shopRepository.UpdateShopifyUninstallId(shopOwnerUserId, webhook.Id);
            return newShop;
        }

        public void UpdateShop(string userId, string currencySymbol, string timezone)
        {
            var pwShop = _shopRepository.RetrieveByUserId(userId);
            pwShop.CurrencyId = _currencyService.AbbreviationToCurrencyId(currencySymbol); ;
            pwShop.TimeZone = timezone;

            using (var transaction = _shopRepository.InitiateTransaction())
            {
                _shopRepository.Update(pwShop);
                _shopRepository.UpdateIsAccessTokenValid(pwShop.PwShopId, true);
                _shopRepository.UpdateIsProfitWiseInstalled(pwShop.PwShopId, true, null);
                transaction.Commit();
            }

            _logger.Debug($"Updated Shop - UserId: {pwShop.ShopOwnerUserId}");
        }


        // Recurring Application Charge methods
        public static RecurringApplicationCharge MakeNewProfitWiseCharge()
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
            var chargeParameter = MakeNewProfitWiseCharge();
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
            var charge = SyncAndRetrieveCurrentCharge(userId);
            var shop = _shopRepository.RetrieveByUserId(userId);

            if (charge.IsValid)
            {
                _shopRepository.UpdateIsBillingValid(shop.PwShopId, true);
                _hangFireService.AddOrUpdateInitialShopRefresh(userId);
                return true;
            }
            else
            {
                var billingRepository = _factory.MakeBillingRepository(shop);
                billingRepository.ClearPrimary();

                _shopRepository.UpdateIsBillingValid(shop.PwShopId, false);
                return false;
            }
        }

        public bool SyncAndValidateBilling(PwShop shop)
        {
            var charge = SyncAndRetrieveCurrentCharge(shop.ShopOwnerUserId);

            if (charge == null)
            {
                _shopRepository.UpdateIsBillingValid(shop.PwShopId, false);
                return false;
            }
            if (charge.LastStatus == ChargeStatus.Active)
            {
                _shopRepository.UpdateIsBillingValid(shop.PwShopId, true);
                return true;
            }
            if (charge.LastStatus == ChargeStatus.Accepted)
            {
                ActivateCharge(shop.ShopOwnerUserId, charge.PwChargeId);
                return true;
            }

            // Charge Status is neither Active nor Accepted, thus set Billing Valid to false
            _shopRepository.UpdateIsBillingValid(shop.PwShopId, false);
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
            billingRepository.Update(pwCharge);
            billingRepository.UpdatePrimary(pwCharge.PwChargeId);

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

            // Cancel the Charge in Shopify
            var credentials = _credentialService.Retrieve(userId).ToShopifyCredentials();
            var apiRepository = _apifactory.MakeRecurringApiRepository(credentials);
            apiRepository.CancelCharge(charge.ShopifyRecurringChargeId);

            // Retrieve the Charge from Shopify API
            var result = apiRepository.RetrieveCharge(charge.ShopifyRecurringChargeId);

            // Update ProfitWise's database record
            charge.ConfirmationUrl = result.confirmation_url;
            charge.LastStatus = result.status.ToChargeStatus();
            charge.LastJson = result.SerializeToJson();
            charge.IsPrimary = false;
            billingRepository.Update(charge);            
        }


        // Uninstallation process
        public void UninstallShop(long shopifyShopId)
        {
            var shop = _shopRepository.RetrieveByShopifyShopId(shopifyShopId);
            _shopRepository.UpdateIsProfitWiseInstalled(shop.PwShopId, false, DateTime.Now);
        }

        public void FinalizeUninstallation(int pwShopId)
        {
            var shop = _shopRepository.RetrieveByShopId(pwShopId);
            var batchRepository = _factory.MakeBatchStateRepository(shop);
            var billingRepository = _factory.MakeBillingRepository(shop);

            // Kill the Recurring Shop Refresh Job
            var batchState = batchRepository.Retrieve();
            if (batchState.RoutineRefreshJobId != null)
            {
                _hangFireService.KillRoutineRefresh(batchState.RoutineRefreshJobId);
            }

            // Clear out any ProfitWise Charge records to force creation of a new Charge
            billingRepository.ClearPrimary();
            _shopRepository.UpdateIsBillingValid(pwShopId, false);
        }
    }
}

