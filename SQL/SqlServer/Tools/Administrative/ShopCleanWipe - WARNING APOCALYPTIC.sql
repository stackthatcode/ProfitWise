

/**** A P O C A L Y P S E - S C R I P T ****/

/**** WARNING - This script will completely destroy a store's data ****/
 
USE ProfitWise
GO


/*** Insert the appropriate PwShopId and uncomment the below line ***/
DECLARE @PwShopId bigint = 100241;

UPDATE batchstate(@PwShopId) SET ProductsLastUpdated = NULL, OrderDatasetStart = NULL, OrderDatasetEnd = NULL;

--DELETE FROM reportquerystub(@PwShopId);
DELETE FROM reportfilter(@PwShopId);
DELETE FROM report(@PwShopId);
DELETE FROM profitreportentry(@PwShopId);

DELETE FROM goodsonhandquerystub(@PwShopId);
DELETE FROM profitquerystub(@PwShopId);

DELETE FROM ordertag(@PwShopId);
DELETE FROM orderrefund(@PwShopId);
DELETE FROM orderlineitem(@PwShopId);
DELETE FROM orderadjustment(@PwShopId);
DELETE FROM ordertable(@PwShopId);

DELETE FROM picklistmasterproduct(@PwShopId);
DELETE FROM picklist(@PwShopId);

DELETE FROM variant(@PwShopId);
DELETE FROM mastervariantcogscalc(@PwShopId);
DELETE FROM mastervariantcogsdetail(@PwShopId);
DELETE FROM mastervariant(@PwShopId);
DELETE FROM product(@PwShopId);
DELETE FROM masterproduct(@PwShopId);

DELETE FROM uploads(@PwShopId);

DECLARE @UserId nvarchar(128);

/*
DELETE FROM recurringcharge(@PwShopId);

SELECT @UserId = ShopOwnerUserId FROM shop(@PwShopId);

DELETE FROM tour(@PwShopId);
DELETE FROM shop(@PwShopId);

DELETE FROM AspNetUserClaims WHERE UserId = @UserId;
DELETE FROM AspNetUserLogins WHERE UserId = @UserId;
DELETE FROM AspNetUserRoles WHERE UserId = @UserId;
DELETE FROM AspNetUsers WHERE Id = @UserId;
*/

