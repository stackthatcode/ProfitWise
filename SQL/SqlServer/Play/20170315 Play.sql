USE ProfitWise
GO


SELECT * FROM profitwiseshop

UPDATE profitwiseshop SET IsDataLoaded = 1;

UPDATE recurringcharge(100001) SET ShopifyRecurringChargeId = 99999;



/**

1.) Run this with the appropriate PwShopId

UPDATE batchstate(100001) SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;
DELETE FROM  profitreportentry(100001);

2.) In the Admin Console, click Kill Batch Jobs

3.) Run the Orders DDL

*/