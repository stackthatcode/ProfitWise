

-- 1.) In the Admin Console, click Kill Batch Jobs

-- 2.) Insert the appropriate PwShopId and execute 

USE ProfitWise
GO


DECLARE @PwShopId bigint = 100099;

UPDATE batchstate(@PwShopId) SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;

DELETE FROM profitreportentry(@PwShopId);

DELETE FROM orderrefund(@PwShopId);
DELETE FROM orderlineitem(@PwShopId);
DELETE FROM orderadjustment(@PwShopId);
DELETE FROM ordertable(@PwShopId);

SELECT * FROM batchstate(@PwShopId);



