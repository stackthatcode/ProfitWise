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


SELECT * FROM exchangerate

SELECT * FROM AspNetUsers;

SELECT * FROM AspNetUserClaims;

SELECT * FROM AspNetUserLogins;

SELECT * FROM profitwiseshop;

SELECT * FROM profitwiserecurringcharge;

UPDATE profitwiserecurringcharge SET LastStatus = 7;

UPDATE profitwiseshop SET IsBillingValid = 0;

SELECT * FROM profitwiseshop;

SELECT * FROM profitwisebatchstate;

-- IsAccessTokenValid, IsShopEnabled, IsBillingValid, IsDataLoaded




DECLARE @PwShopId bigint = 100001

SELECT t3.*
FROM mastervariant(@PwShopId) t1 
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN orderlineitem(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	           
WHERE t1.PwMasterVariantId = 1338;



DECLARE @CogsAmount decimal (15, 2) = 500
DECLARE @PwShopId bigint = 100001
DECLARE @PwMasterVariantId bigint = 1338

SELECT (@CogsAmount * ISNULL(t4.Rate, 0))                                 
FROM mastervariant(@PwShopId) t1                 
	INNER JOIN variant(@PwShopId) t2                
		ON t1.PwMasterVariantId = t2.PwMasterVariantId                
	INNER JOIN orderlineitem(@PwShopId) t3                
		ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId                               
	LEFT JOIN exchangerate t4                
		ON t3.OrderDate = t4.[Date]                 
		AND t4.SourceCurrencyId = 1                
		AND t4.DestinationCurrencyId = 1                
WHERE t1.PwShopId = @PwShopId AND t1.PwMasterVariantId = @PwMasterVariantId

