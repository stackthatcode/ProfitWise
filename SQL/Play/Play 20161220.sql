USE profitwise;


SELECT * FROM profitwiseshop;

SELECT * FROM profitwisereportquerystub WHERE PwReportId = 99739;



SELECT 	t1.PwReportId, t1.PwShopId, t2.PwMasterVariantId, t2.PwProductId, t2.PwVariantId, 
		t3.OrderDate, t3.ShopifyOrderId, t3.ShopifyOrderLineId, t3.Quantity, t3.TotalRestockedQuantity, 
        t3.UnitPrice, t3.GrossRevenue, t4.OrderNumber
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId 
			AND t1.PwShopId = t2.PwShopId
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwProductId = t3.PwProductId 
			AND t2.PwVariantId = t3.PwVariantId
			AND t2.PwShopID = t3.PwShopId
	INNER JOIN shopifyorder t4
		ON t3.ShopifyOrderId = t4.ShopifyOrderId
			AND t3.PwShopID = t4.PwShopId
WHERE t1.PwReportID = 99739;





### CoGS Update query...

### 

SELECT 	t0.CurrencyId, t1.PwMasterVariantId, t3.ShopifyOrderLineId, t3.OrderDate, t1.CogsCurrencyId, t1.CogsAmount, t4.Rate
FROM profitwiseshop t0
    INNER JOIN profitwisemastervariant t1 
		ON t0.PwShopId = t1.PwShopId
	INNER JOIN profitwisevariant t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId AND t1.PwShopId = t2.PwShopId
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId AND t2.PwShopID = t3.PwShopId        
	LEFT JOIN exchangerate t4
		ON Date(t3.OrderDate) = t4.`Date` 
			AND t4.SourceCurrencyId = t1.CogsCurrencyId
			AND t4.DestinationCurrencyId = t0.CurrencyId;



SELECT * FROM profitwiseshop;

SELECT * FROM profitwisemastervariant;

UPDATE profitwisemastervariant SET CogsCurrencyId = 1, CogsAmount = 20;


SELECT * FROM profitwisemastervariant;

SELECT * FROM shopifyorderlineitem;

SELECT * FROM exchangerate;


### Enter the bloody calendar table, you savages!!!
SELECT * FROM calendar_table;

SELECT * FROM shopifyorderlineitem 

SELECT PwMasterVariantId 
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2 ON t1.





