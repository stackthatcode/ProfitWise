USE ProfitWise
GO


SELECT * FROM profitwisebatchstate


DROP VIEW IF EXISTS batchstate
GO

CREATE VIEW batchstate
AS
SELECT * FROM profitwisebatchstate
WHERE PwShopId = USER
GO


-- Example of SQL Server's LEFT OUTER JOIN quirkiness...
SELECT t1.PwMasterVariantId, t1.PwShopId, t2.PwMasterVariantId, t2.PwShopId 
FROM profitwisemastervariant t1
	LEFT JOIN profitwisemastervariantcogsdetail t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId
WHERE t1.PwShopId = 100001 AND t2.PwShopId = 100001

-- How to work around this quirkiness...
SELECT t1.PwMasterVariantId, t1.PwShopId, t2.PwMasterVariantId, t2.PwShopId 
FROM profitwisemastervariant t1
	LEFT JOIN profitwisemastervariantcogsdetail t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId AND t2.PwShopId = 100001
WHERE t1.PwShopId = 100001 




-- Now, using these User-Defined Table Value functions, let's compare execution plans
DROP FUNCTION IF EXISTS dbo.mastervariantcogsdetail
GO


SELECT * FROM AspNetUsers;

SELECT * FROM AspNetUserClaims;

SELECT * FROM AspNetUserLogins;

SELECT * FROM profitwiseshop;

SELECT * FROM profitwiserecurringcharge;

UPDATE profitwiserecurringcharge SET LastStatus = 7;

UPDATE profitwiseshop SET IsBillingValid = 0;

SELECT * FROM profitwiseshop;

SELECT * FROM profitwisebatchstate;


IsAccessTokenValid, IsShopEnabled, IsBillingValid, IsDataLoaded
