
--SELECT * FROM profitreportentry(100001);


-- #1 - Add flag to the PW Ledger
IF COL_LENGTH('dbo.profitwiseprofitreportentry', 'IsNonZeroValue') IS NULL
BEGIN
	ALTER TABLE profitwiseprofitreportentry ADD IsNonZeroValue tinyint NULL;
END
GO



-- #2 - Update PW Ledger for non-zero/zero value Orders
WITH CTE ( ShopifyOrderId, Total ) AS
(
	SELECT ShopifyOrderId, SUM(TotalAfterAllDiscounts) 
	FROM shopifyorderlineitem
	GROUP BY ShopifyOrderId
)
UPDATE t2
SET IsNonZeroValue = CASE WHEN Total <> 0 THEN 1 ELSE 0 END
FROM CTE t1 INNER JOIN 
	profitwiseprofitreportentry t2
		ON t1.ShopifyOrderId = t2.ShopifyOrderId;



-- #3 - Correct IsNonZeroValue column data type
ALTER TABLE profitwiseprofitreportentry ALTER COLUMN IsNonZeroValue tinyint NOT NULL;



-- #4 - Report Preference
IF COL_LENGTH('dbo.profitwiseshop', 'MinIsNonZeroValue') IS NULL
BEGIN
	ALTER TABLE profitwiseshop ADD MinIsNonZeroValue tinyint NULL;
END
GO

UPDATE profitwiseshop SET MinIsNonZeroValue = 0;

ALTER TABLE profitwiseshop ALTER COLUMN MinIsNonZeroValue tinyint NOT NULL;




-- Playspace for C# generated query
DECLARE @PwShopId int = 100001;

WITH CTE ( ShopifyOrderId, Total ) AS
(
	SELECT ShopifyOrderId, SUM(TotalAfterAllDiscounts) 
	FROM orderlineitem(@PwShopId)
	GROUP BY ShopifyOrderId
)
UPDATE t2
SET IsNonZeroValue = CASE WHEN Total <> 0 THEN 1 ELSE 0 END
FROM CTE t1 INNER JOIN 
	profitreportentry(@PwShopId) t2
		ON t1.ShopifyOrderId = t2.ShopifyOrderId

-- Locate all zero value orders
WITH CTE ( ShopifyOrderId, Total ) AS
(
	SELECT ShopifyOrderId, SUM(NetSales) 
	FROM profitreportentry(100001) GROUP BY ShopifyOrderId
)
SELECT * FROM CTE WHERE Total = 0;


SELECT * FROM profitreportentry(100001) WHERE IsNonZeroValue = 0;



