USE ProfitWise
GO


SELECT * FROM profitwisemastervariant;


DECLARE @Today DateTime;
SELECT @Today = DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)
SELECT * FROM exchangerate WHERE [Date] = @Today;



DECLARE @QueryDate DateTime = '2016-01-01'
--SELECT @Today = DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)

SELECT	t1.PwMasterVariantId, t2.PwProductId, t2.PwVariantId, t2.Inventory, t2.LowPrice, t2.HighPrice, 
		t4.PercentMultiplier * t2.HighPrice + t4.FixedAmount * t5.Rate AS CostOfGoodsOnHand
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId	
	LEFT JOIN profitwisemastervariantcogscalc t4 
		ON t1.PwShopId = t4.PwShopId AND t1.PwMasterVariantId = t4.PwMasterVariantId 
		AND t4.StartDate <= @QueryDate AND t4.EndDate > @QueryDate
	LEFT JOIN exchangerate t5 ON t4.SourceCurrencyId = t5.SourceCurrencyId
			AND t5.Date = @QueryDate AND t5.DestinationCurrencyId = 1 
WHERE t1.PwShopId = 100001
AND t1.StockedDirectly = 1
AND t2.PwVariantId IN ( SELECT PwVariantId FROM profitwisegoodsonhandquerystub WHERE PwShopId = 100001 AND PwReportId = 2 )
AND t2.PwShopId = 100001
AND t2.Inventory IS NOT NULL
AND t2.IsActive = 1



