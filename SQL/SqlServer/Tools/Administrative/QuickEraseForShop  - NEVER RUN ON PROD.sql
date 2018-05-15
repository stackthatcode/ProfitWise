
USE ProfitWise
GO

-- This script will rename a Shop's Domain in ASP.NET Users, thereby preventing PW from verifying their existence

DECLARE @PwShopId int = 100091;
DECLARE @NewShopifyShopId bigint = 16666286590; -- Set this to prevent collisions when reinstalling

UPDATE t1
SET UserName = UserName + 'DISABLED'
FROM AspNetUsers t1
	INNER JOIN shop(@PwShopId) t2
		ON t1.Id = t2.ShopOwnerUserId;

UPDATE shop(@PwShopId) SET Domain = Domain + 'DISABLED', ShopifyShopId = @NewShopifyShopId;

DELETE FROM AspNetUserClaims WHERE UserId IN 
	( SELECT Id FROM AspNetUsers t1
		INNER JOIN shop(@PwShopId) t2
			ON t1.Id = t2.ShopOwnerUserId );

DELETE FROM AspNetUserLogins WHERE UserId IN 
	( SELECT Id FROM AspNetUsers t1
		INNER JOIN shop(@PwShopId) t2
			ON t1.Id = t2.ShopOwnerUserId );

