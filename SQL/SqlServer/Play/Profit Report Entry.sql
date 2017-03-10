


DECLARE @PwShopId int = 100001;
DECLARE @OrderLineEntry int = 1, @RefundEntry int = 2, @AdjustmentEntry int = 3;


DELETE FROM profitreportentry(@PwShopId);


INSERT INTO profitreportentry(@PwShopId)
SELECT PwShopId, OrderDate, @OrderLineEntry AS EntryType, ShopifyOrderId, ShopifyOrderLineId AS SourceId,                     
	PwProductId, PwVariantId, TotalAfterAllDiscounts AS NetSales, 
	Quantity * ISNULL(UnitCogs, 0) AS CoGS, 	
	Quantity AS Quantity, 
	CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END AS PaymentStatus                        
FROM orderlineitem(@PwShopId);


INSERT INTO profitreportentry(@PwShopId)                    
SELECT t1.PwShopId, t1.RefundDate, @RefundEntry AS EntryType, t1.ShopifyOrderId, t1.ShopifyRefundId AS SourceId,                     
	t1.PwProductId, t1.PwVariantId, -t1.Amount AS NetSales, 
	-t1.RestockQuantity * ISNULL(UnitCoGS, 0) AS CoGS, 	
	-t1.RestockQuantity AS Quantity, 
	CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END AS PaymentStatus                        
FROM orderrefund(@PwShopId) t1                
	INNER JOIN orderlineitem(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.ShopifyOrderLineId = t2.ShopifyOrderLineId 


INSERT INTO profitreportentry(@PwShopId)
SELECT t1.PwShopId, t1.AdjustmentDate, @AdjustmentEntry AS EntryType, t1.ShopifyOrderId,                     
	t1.ShopifyAdjustmentId AS SourceId, NULL, NULL, t1.Amount AS NetSales, 
	0 AS CoGS, 
	NULL AS Quantity,
	CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN 2 ELSE 1 END AS PaymentStatus                        
FROM orderadjustment(@PwShopId) t1                     
	INNER JOIN ordertable(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId;




/*
SELECT * FROM profitwiseshop;

SELECT * FROM orderlineitem(100001);
SELECT t1.*, t2.PwMasterVariantId, t3.PwMasterProductId
FROM orderlineitem(@PwShopId) t1
	INNER JOIN variant(@PwShopID) t2 ON t1.PwVariantId = t2.PwVariantId
	INNER JOIN mastervariant(@PwShopID) t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId;
*/

/*
SELECT * FROM profitwiseshop;

UPDATE profitwiseshop SET ProfitRealization = 1;

SELECT * FROM dbo.profitreportentry(100001) WHERE PaymentCleared = 0;
*/

