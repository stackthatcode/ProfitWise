
USE ProfitWise
GO

-- This script will rename a Shop's Domain in ASP.NET Users, thereby preventing PW from verifying their existence

DECLARE @PwShopId int = 999999;

UPDATE t1
SET UserName = UserName + 'DISABLED'
FROM AspNetUsers t1
	INNER JOIN shop(@PwShopId) t2
		ON t1.Id = t2.ShopOwnerUserId;


