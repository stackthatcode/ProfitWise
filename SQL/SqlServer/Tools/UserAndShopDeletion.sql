USE ProfitWise
GO



DECLARE @UserId varchar(128) = 'bb74e21c-a5ba-4523-8ee6-19a3ee4fde9b';

DECLARE @PwShopId bigint;
SELECT @PwShopId = PwShopId FROM profitwiseshop WHERE ShopOwnerUserId = @UserId;


/** Uncomment to Delete!!
DELETE FROM AspNetUsers WHERE Id = @UserId;
DELETE FROM AspNetUserClaims WHERE UserId = @UserId;
DELETE FROM AspNetUserRoles WHERE UserId = @UserId;
DELETE FROM AspNetUserLogins WHERE UserId = @UserId;

DELETE FROM shop(@PwShopId);
DELETE FROM batchstate(@PwShopId);
DELETE FROM recurringcharge(@PwShopId);
**/

SELECT * FROM AspNetUsers WHERE Id = @UserId;
SELECT * FROM AspNetUserClaims WHERE UserId = @UserId;
SELECT * FROM AspNetUserRoles WHERE UserId = @UserId;
SELECT * FROM AspNetUserLogins WHERE UserId = @UserId;

SELECT * FROM shop(@PwShopId);
SELECT * FROM batchstate(@PwShopId);
SELECT * FROM recurringcharge(@PwShopId);
