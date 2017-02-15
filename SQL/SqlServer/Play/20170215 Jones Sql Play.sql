USE ProfitWise
GO


SELECT * FROM [profitwisemastervariantcogscalc]

DELETE FROM [profitwisemastervariantcogscalc]


SELECT 
	t1.PwMasterProductId, t1.PwMasterVariantId,  t3.Vendor, t3.ProductType, 
	t3.PwProductId, t2.PwVariantId, t3.Title AS ProductTitle, t2.Title AS VariantTitle, 
	
	t2.Inventory, t2.LowPrice, t2.HighPrice, t4.PercentMultiplier * t2.HighPrice + t4.FixedAmount * t5.Rate AS CostOfGoodsOnHand

FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN profitwiseproduct t3 ON t2.PwProductId = t3.PwProductId
	INNER JOIN profitwisemastervariantcogscalc t4 ON t1.PwMasterVariantId = t4.PwMasterVariantId
	LEFT JOIN exchangerate t5 ON t4.SourceCurrencyId = t5.SourceCurrencyId

WHERE t1.PwShopId = 100001
AND t1.StockedDirectly = 1
AND t1.PwMasterVariantId IN ( 
	SELECT PwMasterVariantId FROM profitwisereportquerystub WHERE PwShopId = 100001 AND PwReportId = 1 )

AND t2.PwShopId = 100001
AND t2.Inventory IS NOT NULL
AND t2.IsActive = 1

AND t3.PwShopId = 100001
AND t4.PwShopId = 100001
AND t5.DestinationCurrencyId = 1 
AND t5.Date = '2017-02-01'



SELECT * FROM profitwisereportquerystub;
