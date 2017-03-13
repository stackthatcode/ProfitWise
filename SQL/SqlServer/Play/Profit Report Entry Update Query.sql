


-- #1 - Basic Query for Order Line
DECLARE @PwShopId int = 100001;



-- Order

SELECT * FROM mastervariant(100001) WHERE PwMasterProductId = 72;

SELECT * FROM variant(100001) WHERE PwMasterVariantId = 362;

SELECT * FROM orderlineitem(100001) WHERE PwVariantId = 364;

SELECT * FROM profitreportentry(100001) WHERE PwVariantId = 364;

 ORDER BY UnitCoGS DESC;



DECLARE @PwShopId bigint = 100001;

SELECT pr.*
FROM profitreportentry(@PwShopId) pr
	INNER JOIN orderrefund(@PwShopId) orf
		ON pr.ShopifyOrderId = orf.ShopifyOrderId 
        AND pr.SourceId = orf.ShopifyRefundId
	INNER JOIN orderlineitem(@PwShopId) oli
		ON orf.ShopifyOrderId = oli.ShopifyOrderId 
        AND orf.ShopifyOrderLineId = oli.ShopifyOrderLineId
WHERE pr.ShopifyOrderId = 4466752978;


SELECT * FROM profitreportentry(100001) WHERE ShopifyOrderId = 4466752978;

SELECT * FROM orderrefund(100001) WHERE ShopifyOrderId = 4466752978;

SELECT * FROM orderlineitem(100001) WHERE ShopifyOrderId = 4466752978;




SELECT * FROM profitreportentry(100001) WHERE ShopifyOrderId = 4466752978;


SELECT SUM(Quantity * UnitPrice), SUM(Quantity * UnitCogs) FROM orderlineitem(100001)



SELECT * FROM profitreportentry(100001);


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





SELECT pr.*, mv.PwMasterVariantId, mv.PwMasterVariantId 
FROM profitreportentry(100001) pr
	LEFT JOIN variant(100001) v
		ON pr.PwVariantId = v.PwVariantId
	LEFT JOIN mastervariant(100001) mv
		ON v.PwMasterVariantId = mv.PwMasterVariantId;

SELECT * FROM mastervariant(100001) WHERE PwMasterVariantId = 542;

DELETE FROM profitreportentry(@PwShopId) ; 

