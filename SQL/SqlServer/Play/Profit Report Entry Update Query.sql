


-- #1 - Basic Query for Order Line
DECLARE @PwShopId int = 100001;



-- Order
SELECT * FROM orderlineitem(100001);

SELECT * FROM profitreportentry(100002);


UPDATE t1
SET CoGS = t1.Quantity * ISNULL(UnitCogs, 0),
	PaymentStatus = CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END
FROM profitreportentry(@PwShopId) t1
	INNER JOIN orderlineitem(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.SourceId = t2.ShopifyOrderLineId


WHERE t1.ShopifyOrderId = 277050777


-- Master Variant
UPDATE t1
SET CoGS = t1.Quantity * ISNULL(UnitCogs, 0),
	PaymentStatus = CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END
FROM profitreportentry(@PwShopId) t1
	INNER JOIN orderlineitem(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.SourceId = t2.ShopifyOrderLineId
	INNER JOIN variant(@PwShopId) t3 ON t2.PwVariantId = t3.PwVariantId	
WHERE t3.PwMasterVariantId = 5;


-- Master Product
UPDATE t1
SET CoGS = t1.Quantity * ISNULL(UnitCogs, 0),
	PaymentStatus = CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END
FROM profitreportentry(@PwShopId) t1
	INNER JOIN orderlineitem(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.SourceId = t2.ShopifyOrderLineId
	INNER JOIN variant(@PwShopId) t3 ON t2.PwVariantId = t3.PwVariantId
	INNER JOIN mastervariant(@PwShopId) t4 ON t3.PwMasterVariantId = t4.PwMasterVariantId
WHERE t4.PwMasterProductId = 2;

-- Pick List
UPDATE t1
SET CoGS = t1.Quantity * ISNULL(UnitCogs, 0),
	PaymentStatus = CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END
FROM profitreportentry(@PwShopId) t1
	INNER JOIN orderlineitem(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.SourceId = t2.ShopifyOrderLineId
	INNER JOIN variant(@PwShopId) t3 ON t2.PwVariantId = t3.PwVariantId
	INNER JOIN mastervariant(@PwShopId) t4 ON t3.PwMasterVariantId = t4.PwMasterVariantId
WHERE t4.PwMasterProductId IN ( SELECT PwMasterProductId FROM picklistmasterproduct(@PwShopId) WHERE PwPickListId = 110403 );



SELECT * FROM orderlineitem(100001);



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
--DECLARE @PwShopId int = 100001;

UPDATE t1
SET PaymentStatus = CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END 
FROM profitreportentry(@PwShopId) t1
	INNER JOIN ordertable(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE t1.EntryType = 3





SELECT * FROM profitreportentry(100001)


DELETE FROM profitreportentry(@PwShopId) ; 


INSERT INTO profitreportentry(@PwShopId)                
SELECT PwShopId, OrderDate, @OrderLineEntry AS EntryType, ShopifyOrderId, ShopifyOrderLineId AS SourceId,                 PwProductId, PwVariantId, TotalAfterAllDiscounts AS NetSales,                         Quantity * ISNULL(UnitCogs, 0) AS CoGS,                        Quantity AS Quantity, CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2             ELSE 1 END AS PaymentStatus  FROM orderlineitem(@PwShopId) ; INSERT INTO profitreportentry(@PwShopId)                SELECT t1.PwShopId, t1.RefundDate, @RefundEntry AS EntryType, t1.ShopifyOrderId, t1.ShopifyRefundId AS SourceId,                 t1.PwProductId, t1.PwVariantId, -t1.Amount AS NetSales,                         -t1.RestockQuantity * ISNULL(UnitPrice, 0) AS CoGS,                     -t1.RestockQuantity AS Quantity, CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2             ELSE 1 END AS PaymentStatus FROM orderrefund(@PwShopId) t1            INNER JOIN orderlineitem(@PwShopId) t2            ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.ShopifyOrderLineId = t2.ShopifyOrderLineId ; INSERT INTO profitreportentry(@PwShopId)                SELECT t1.PwShopId, t1.AdjustmentDate, @AdjustmentEntry AS EntryType, t1.ShopifyOrderId,                     t1.ShopifyAdjustmentId AS SourceId, NULL, NULL, t1.Amount AS NetSales,                     0 AS CoGS, NULL AS Quantity, CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2             ELSE 1 END AS PaymentStatus FROM orderadjustment(@PwShopId) t1                     INNER JOIN ordertable(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId ; "

SELECT * FROM

