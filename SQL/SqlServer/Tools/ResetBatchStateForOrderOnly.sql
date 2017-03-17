

-- 1.) In the Admin Console, click Kill Batch Jobs

-- 1.) Insert the appropriate PwShopId and execute 

USE ProfitWise
GO


DECLARE @PwShopId bigint = 100003;
UPDATE batchstate(@PwShopId) SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;
DELETE FROM  profitreportentry(@PwShopId);
DELETE FROM orderrefund(100003);
DELETE FROM orderlineitem(100003);
DELETE FROM orderadjustment(100003);
DELETE FROM ordertable(100003);


-- 3.) Run the Orders DDL

SELECT * FROM profitwiseshop;

SELECT * FROM batchstate(100001);

sp_who


use master
ALTER DATABASE ProfitWise SET SINGLE_USER WITH ROLLBACK IMMEDIATE 

--do you stuff here 

ALTER DATABASE ProfitWise SET MULTI_USER



