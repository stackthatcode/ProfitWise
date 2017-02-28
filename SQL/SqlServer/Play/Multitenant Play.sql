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

CREATE FUNCTION dbo.mastervariantcogsdetail(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN  
SELECT * FROM profitwisemastervariantcogsdetail WHERE PwShopId = @PwShopId;
GO


DECLARE @PwShopId bigint = 100001;
SELECT  @PwShopId;
SELECT * FROM mastervariantcogsdetail(@PwShopId);

DECLARE @PwShopId2 bigint;
SELECT  @PwShopId2;
SELECT * FROM mastervariantcogsdetail(@PwShopId2);


DECLARE @PwShopId bigint = 100001;

SELECT t1.PwMasterVariantId, t1.PwShopId, t2.PwMasterVariantId, t2.PwShopId 
FROM mastervariant(@PwShopId) t1
	LEFT JOIN mastervariantcogsdetail(@PwShopId) t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId;



-- Compare that ^^^^ with this:
SELECT t1.PwMasterVariantId, t1.PwShopId, t2.PwMasterVariantId, t2.PwShopId 
FROM profitwisemastervariant t1
	LEFT JOIN profitwisemastervariantcogsdetail t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId AND t2.PwShopId = 100001
WHERE t1.PwShopId = 100001 



SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG='ProfitWise'
ORDER BY TABLE_NAME;

SELECT * FROM profitwisebatchstate HWERE

SELECT * FROM batchstate(100003);


SELECT * FROM AspNetRoles;


SELECT t1.Id, UserName, Email, t4.TimeZone, t4.Domain, t4.CurrencyId, t4.
FROM AspNetUsers t1 
	INNER JOIN AspNetUserRoles t2 ON t1.Id = t2.UserId
	INNER JOIN AspNetRoles t3 ON t2.RoleId = t3.Id AND t3.Name = 'USER'
	INNER JOIN profitwiseshop t4 ON t1.Id = t4.ShopOwnerUserId;


SELECT * FROM AspNetUsers;

SELECT * FROM AspNetUserRoles;

SELECT * FROM profitwiseshop;

