
--SELECT * FROM profitreportentry(100001);


-- #1 - Add flag to the PW Ledger
IF COL_LENGTH('dbo.profitwiseprofitreportentry', 'IsNonZeroValue') IS NULL
BEGIN
	ALTER TABLE profitwiseprofitreportentry ADD IsNonZeroValue tinyint NULL;
END
GO



-- #2A - Update PW Ledger for non-zero value Orders
WITH CTE ( ShopifyOrderId, Total ) AS
(
	SELECT ShopifyOrderId, SUM(TotalAfterAllDiscounts) 
	FROM shopifyorderlineitem
	GROUP BY ShopifyOrderId
)
UPDATE t2
SET IsNonZeroValue = 1
FROM CTE t1 INNER JOIN 
	profitwiseprofitreportentry t2
		ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE t1.Total <> 0;



-- #2B - Update PW Ledger for zero value Orders
WITH CTE ( ShopifyOrderId, Total ) AS
(
	SELECT ShopifyOrderId, SUM(TotalAfterAllDiscounts) 
	FROM shopifyorderlineitem
	GROUP BY ShopifyOrderId
)
UPDATE t2
SET IsNonZeroValue = 0
FROM CTE t1 INNER JOIN 
	profitwiseprofitreportentry t2
		ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE t1.Total = 0;



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


