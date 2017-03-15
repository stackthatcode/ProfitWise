USE ProfitWise
GO



DECLARE @UserId varchar(128) = 'fc519160-aba2-46fb-81a8-9bf87b6a9fb8';

DECLARE @PwShopId bigint;
SELECT @PwShopId = PwShopId FROM profitwiseshop WHERE ShopOwnerUserId = @UserId;

SELECT * FROM AspNetUsers WHERE UserId = @UserId;

SELECT * FROM AspNetUserClaims WHERE UserId = @UserId;

SELECT * FROM AspNetUserRoles WHERE UserId = @UserId;

SELECT * FROM AspNetUserLogins WHERE UserId = @UserId;

SELECT * FROM shop(

SELECT * FROM batchstate(1;

SELECT * FROM recurringcharge
