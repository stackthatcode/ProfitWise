USE profitwise;


SELECT SUM(GrossRevenue) FROM shopifyorderlineitem WHERE OrderDate = '2016-12-05';
SELECT * FROM shopifyorderlineitem WHERE OrderDate = '2016-12-02';

# ProfitWise says => 1535.06
# Shopify says => 1590.67

UPDATE profitwisemastervariant SET CoGSAmount = NULL;
##UPDATE profitwisemastervariant SET CurrencyId = 1, CogsAmount = 0 WHERE CogsAmount = NULL


## SAVE *** Update all the CoGS based on 20% margin
UPDATE profitwiseshop t0
    INNER JOIN profitwisemastervariant t1 
		ON t0.PwShopId = t1.PwShopId
	INNER JOIN profitwisevariant t2 
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId AND t2.IsPrimary = 1
SET t1.CogsCurrencyId = t0.CurrencyId, t1.CogsAmount = t2.HighPrice * 0.80, t1.CogsDetail = false
WHERE t0.PwShopId = 100001
AND t1.CogsAmount IS NULL;

#$1000 @ 20% Margin = $800
#$800 @ 20% ROI => $960

## SAVE *** Update all the Order Line CoGS
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
WHERE t0.PwShopId = 100001;




SELECT
	SUM(t3.TotalAfterAllDiscounts) As TotalRevenue, 
	COUNT(DISTINCT(t3.ShopifyOrderId)) AS TotalNumberSold,
	SUM(t3.UnitCogs * t3.Quantity) AS TotalCogs,
	SUM(t3.TotalAfterAllDiscounts) - SUM(t3.UnitCogs * t3.Quantity) AS TotalProfit,
	100.0 - (100.0 * SUM(t3.UnitCogs * t3.NetQuantity) / SUM(t3.TotalAfterAllDiscounts)) AS AverageMargin
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	LEFT JOIN shopifyorderlineitem t3
		ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  
			AND t3.OrderDate >= '2016-12-05' AND t3.OrderDate <= '2016-12-05' 
	LEFT JOIN shopifyorderrefund t4
		ON t1.PwShopId = t4.PwShopId AND t2.PwProductId = t4.PwProductId AND t2.PwVariantId = t4.PwVariantId  
		
SELECT * FROM shopifyorderlineitem;
        
                            
                            

SELECT * FROM calendar_table;
SELECT * FROM shopifyorder;


SELECT t1.Quantity * t1.UnitPrice FROM shopifyorderlineitem t1
	INNER JOIN profitwisevariant t2 ON t1.PwProductId = t2.PwProductId AND t1.PwVariantId = t2.PwVariantId
WHERE OrderDate = '2016-12-05'
ORDER BY Sku;

SELECT 2461.95 - 102.00 - 102.00 - 34.00 -24.28;

SELECT * FROM shopifyorderrefund WHERE RefundDate = '2016-12-05';
SELECT * FROM shopifyorderadjustment WHERE AdjustmentDate = '2016-12-05';

SELECT * FROM shopifyorderlineitem WHERE ShopifyOrderId = 4255498761;
SELECT * FROM shopifyorder WHERE ShopifyOrderId = 4255498761;
SELECT * FROM shopifyorderadjustment;
SELECT * FROM shopifyorderrefund;

SELECT * FROM profitwisereportquerystub;
SELECT * FROM profitwisemastervariant;




SELECT 
	SUM(t1.NetSales) As TotalRevenue, 
	SUM(DISTINCT(IF(t2.ShopifyOrderId IS NULL, 0, 1)))  AS TotalNumberSold, 
	SUM(t1.CoGS) AS TotalCogs, 
	SUM(t1.NetSales) - SUM(t1.CoGS) AS TotalProfit, 
	100.0 - (100.0 * SUM(t1.CoGS) / SUM(t1.NetSales)) AS AverageMargin
FROM profitwiseprofitreportentry t1
	LEFT OUTER JOIN shopifyorder t2
		ON t1.PwShopId = t2.PwShopId AND t1.ShopifyOrderId = t2.ShopifyOrderId



SELECT * FROM profitwiseprofitreportentry 
WHERE EntryDate >= '2016-08-01' 
AND EntryDate <= '2016-08-31' 
AND EntryType = 3 
AND ShopifyOrderID = SourceId;


SELECT SUM(NetSales)
FROM profitwiseprofitreportentry 
WHERE EntryDate >= '2016-08-01' 
AND EntryDate <= '2016-08-31' 
AND EntryType = 3 
AND ShopifyOrderID = SourceId;


SELECT COUNT(*) AS NumberOfOrders FROM shopifyorder
WHERE OrderDate >= '2016-12-01' AND OrderDate <= '2016-12-31' AND Cancelled = 0


SELECT * FROM profitwiseprofitreportentry;

SELECT * FROM shopifyorderrefund;

SELECT * FROM shopifyorder WHERE OrderDate >= '2016-12-01' AND OrderDate <= '2016-12-31'

SELECT * FROM profitwiseprofitreportentry WHERE ShopifyOrderId = 4444204050;


SELECT COUNT(DISTINCT(t3.ShopifyOrderId)) 
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN profitwiseprofitreportentry t3
		ON t1.PwShopId = t3.PwShopId
			AND t2.PwProductId = t3.PwProductId 
			AND t2.PwVariantId = t3.PwVariantId
            AND t3.EntryType = 1
WHERE t1.PwReportId = 99759;

SELECT * FROM profitwiseprofitreportentry;


AND t1.PwShopId = @PwShopId AND t1.PwReportId = @PwReportId 

