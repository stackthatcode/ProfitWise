

-- 1.) Insert the approrioate PwShopId and execute 

DECLARE @PwShopId bigint = 100001;
UPDATE batchstate(@PwShopId) SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;
DELETE FROM  profitreportentry(@PwShopId);

-- 2.) In the Admin Console, click Kill Batch Jobs

-- 3.) Run the Orders DDL