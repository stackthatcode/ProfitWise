


-- #1 - Basic Query for Order Line
DECLARE @PwShopId int = 100001;

UPDATE t1
SET CoGS = t1.Quantity * ISNULL(UnitCogs, 0),
	PaymentStatus = CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END
FROM profitreportentry(@PwShopId) t1
	INNER JOIN orderlineitem(@PwShopId) t2
		ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.SourceId = t2.ShopifyOrderLineId



-- #2 - Basic Query for Updating Refunds
--DECLARE @PwShopId int = 100001;

UPDATE t1
SET t1.CoGS = -t2.RestockQuantity * ISNULL(t3.UnitCoGS, 0),
	t1.PaymentStatus = CASE WHEN t3.FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END
FROM profitreportentry(@PwShopId) t1
	INNER JOIN orderrefund(@PwShopId) t2
		ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.SourceId = t2.ShopifyOrderLineId
	INNER JOIN orderlineitem(@PwShopId) t3
		ON t2.ShopifyOrderId = t3.ShopifyOrderId AND t2.ShopifyOrderLineId = t3.ShopifyOrderLineId



-- #3 - Query for Updating Adjustments
DECLARE @PwShopId int = 100001;

UPDATE t1
SET PaymentStatus = CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END 
FROM profitreportentry(@PwShopId) t1
	INNER JOIN ordertable(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE t1.EntryType = 3


	

/*
SELECT * FROM orderlineitem(100001);

SELECT * FROM orderrefund(100001);
*/

