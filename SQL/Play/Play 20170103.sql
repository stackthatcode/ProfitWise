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
WHERE t0.PwShopId = 100001
AND t3.UnitCogs IS NULL;



SELECT * FROM calendar_table;

SELECT * FROM shopifyorder;
SELECT * FROM shopifyorderlineitem;
SELECT * FROM shopifyorderadjustment;
SELECT * FROM shopifyorderrefund;

SELECT * FROM profitwisereportquerystub;



	SUM(t3.NetTotal) As TotalRevenue, 
	SUM(t3.NetQuantity) AS TotalNumberSold,
	SUM(t3.UnitCogs * t3.NetQuantity) AS TotalCogs,
	SUM(t3.NetTotal) - SUM(t3.UnitCogs * t3.NetQuantity) AS TotalProfit,
	100.0 - (100.0 * SUM(t3.UnitCogs * t3.NetQuantity) / SUM(t3.NetTotal)) AS AverageMargin
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  





