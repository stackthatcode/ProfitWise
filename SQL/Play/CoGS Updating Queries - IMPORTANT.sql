USE profitwise; # Set active database

SET SQL_SAFE_UPDATES = 0; # Turn off Safe Updates setting

#UPDATE profitwisemastervariant SET CoGSAmount = NULL;
##UPDATE profitwisemastervariant SET CurrencyId = 1, CogsAmount = 0 WHERE CogsAmount = NULL


## SAVE *** Update all the CoGS based on 20% margin
#UPDATE profitwiseshop t0
#    INNER JOIN profitwisemastervariant t1 
#		ON t0.PwShopId = t1.PwShopId
#	INNER JOIN profitwisevariant t2 
#		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId AND t2.IsPrimary = 1
#SET t1.CogsCurrencyId = t0.CurrencyId, t1.CogsAmount = t2.HighPrice * 0.80, t1.CogsDetail = false
#WHERE t0.PwShopId = 100001;

#$1000 @ 20% Margin = $800
#$800 @ 20% ROI => $960



## SAVE *** for testing purposes only - bulk update CoGS => Migrate to the Bulk Update Step
UPDATE profitwiseshop t0
	INNER JOIN profitwisemastervariant t1 
		ON t0.PwShopId = t1.PwShopId
	INNER JOIN profitwisevariant t2 
		ON t1.PwShopId = t2.PwShopId 
			AND t1.PwMasterVariantId = t2.PwMasterVariantId 
			AND t2.IsPrimary = 1
SET t1.CogsCurrencyId = t0.CurrencyId, 
	t1.CogsAmount = t2.HighPrice * 0.80, 
	t1.CogsDetail = false
WHERE t1.PwShopId = 100001 
AND t1.CogsAmount = 0;




## SAVE
## 1A *** Update all the Order Line CoGS for Order Line Items with NULL UnitCogs
## 1B *** Update all the Order Line CoGS for Order Line Items tied to MasterVariantId / MasterProductId

UPDATE profitwiseshop t0
    INNER JOIN profitwisemastervariant t1 
		ON t0.PwShopId = t1.PwShopId
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	LEFT JOIN exchangerate t4
		ON Date(t3.OrderDate) = t4.`Date` 
			AND t4.SourceCurrencyId = t1.CogsCurrencyId
			AND t4.DestinationCurrencyId = t0.CurrencyId
SET t3.UnitCogs = (t1.CogsAmount * IFNULL(t4.Rate, 0))
WHERE t0.PwShopId = 100001 
AND t1.PwMasterProductId = 8;

#WHERE t0.PwShopId = 100001 AND t3.UnitCogs IS NULL;



SELECT * FROM profitwisemastervariant;


-- This deletes all
DELETE FROM profitwiseprofitreportentry WHERE PwShopId = 100001;

INSERT INTO profitwiseprofitreportentry
SELECT 	PwShopId, OrderDate, 1 AS EntryType, ShopifyOrderId, ShopifyOrderLineId AS SourceId, 
		PwProductId, PwVariantId, TotalAfterAllDiscounts AS NetSales, Quantity * UnitCogs AS CoGS,
        Quantity AS Quantity
FROM shopifyorderlineitem
WHERE PwShopId = 100001;

INSERT INTO profitwiseprofitreportentry
SELECT 	t1.PwShopId, t1.RefundDate, 2 AS EntryType, t1.ShopifyOrderId, t1.ShopifyRefundId AS SourceId, 
		t1.PwProductId, t1.PwVariantId, -t1.Amount AS NetSales, -t1.RestockQuantity * t2.UnitCogs AS CoGS,
        -t1.RestockQuantity AS Quantity
FROM shopifyorderrefund t1
		INNER JOIN shopifyorderlineitem t2
			ON t1.PwShopId = t2.PwShopId 
            AND t1.ShopifyOrderId = t2.ShopifyOrderId 
            AND t1.ShopifyOrderLineId = t2.ShopifyOrderLineId
WHERE t1.PwShopId = 100001;

INSERT INTO profitwiseprofitreportentry
SELECT t1.PwShopId, t1.AdjustmentDate, 3 AS EntryType, t1.ShopifyOrderId, t1.ShopifyAdjustmentId AS SourceId, 
		NULL, NULL, t1.Amount AS NetSales, 0 AS CoGS, NULL AS Quantity
FROM shopifyorderadjustment t1
WHERE t1.PwShopId = 100001;





