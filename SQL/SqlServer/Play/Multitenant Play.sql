USE ProfitWise
GO


SELECT * FROM profitwisebatchstate

SELECT * FROM exchangerate

SELECT * FROM AspNetUsers;

SELECT * FROM AspNetUserClaims;

SELECT * FROM AspNetUserLogins;

SELECT * FROM profitwiseshop;

SELECT * FROM profitwisebatchstate



SELECT t1.Id AS UserId, UserName, Email, t4.TimeZone, t4.Domain, t4.CurrencyId, 
        t4.PwShopId, t4.IsAccessTokenValid, t4.IsProfitWiseInstalled, t4.IsDataLoaded, 
        t5.ProductsLastUpdated, t4.TempFreeTrialOverride, t6.[LastStatus]
FROM AspNetUsers t1 
	INNER JOIN AspNetUserRoles t2 ON t1.Id = t2.UserId
	INNER JOIN AspNetRoles t3 ON t2.RoleId = t3.Id AND t3.Name = 'USER'
	INNER JOIN profitwiseshop t4 ON t1.Id = t4.ShopOwnerUserId
    LEFT JOIN profitwisebatchstate t5 on t4.PwShopId = t5.PwShopId
    LEFT JOIN profitwiserecurringcharge t6 ON t4.PwShopId = t6.PwShopId AND t6.IsPrimary = 1

