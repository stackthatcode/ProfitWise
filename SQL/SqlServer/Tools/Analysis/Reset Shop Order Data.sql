USE ProfitWise;
GO



-- Reset
DECLARE @PwShopId int = 100001;

DELETE FROM profitreportentry(@PwShopId);

DELETE FROM orderadjustment(@PwShopId);
DELETE FROM orderrefund(@PwShopId);
DELETE FROM orderlineitem(@PwShopId);

DELETE FROM ordertable(@PwShopId);

UPDATE batchstate(@PwShopId) SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;
UPDATE shop(@PwShopId) SET IsDataLoaded = 0;

