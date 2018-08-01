USE ProfitWise;
GO


DROP FUNCTION IF EXISTS dbo.costofgoodsbydate
GO

CREATE FUNCTION dbo.costofgoodsbydate(@PwShopId bigint, @QueryDate datetime)  
RETURNS TABLE  
AS  
RETURN
SELECT	t2.PwProductId, 
		t2.PwVariantId, 
		t2.ShopifyVariantId,
		t2.Title, 
		t2.Sku, 
		dbo.ufnNegToZero(t2.Inventory) AS Inventory,
		t1.PwMasterVariantId, 
		t1.StockedDirectly,
		t1.Exclude,
		t2.IsActive,
		t2.IsPrimary,
		t2.LowPrice,
		t2.HighPrice,
		t2.CurrentPrice AS CurrentUnitPrice,
		CASE WHEN t4.PercentMultiplier <> 0 THEN 100.0 - t4.PercentMultiplier ELSE 0 END AS MarginPercent,
		t4.FixedAmount,
		t4.SourceCurrencyId,
		t6.Abbreviation,
		CASE WHEN (t4.PercentMultiplier = 0 AND t4.FixedAmount = 0 AND DefaultMargin <> 0) THEN (DefaultMargin / 100.0) * t2.HighPrice
        ELSE (ISNULL(t4.PercentMultiplier, 0) / 100 * t2.HighPrice + ISNULL(t4.FixedAmount, 0) * t5.Rate) END AS UnitCogsByDate
FROM shop(@PwShopId) t0
	INNER JOIN mastervariant(@PwShopId) t1
		ON t0.PwShopId = t1.PwShopId
	INNER JOIN variant(@PwShopId) t2 
		ON t1.PwMasterVariantId = t2.PwMasterVariantId	
	INNER JOIN mastervariantcogscalc(@PwShopId) t4 
		ON t1.PwMasterVariantId = t4.PwMasterVariantId
		AND t4.StartDate <= @QueryDate 
		AND t4.EndDate > @QueryDate 
	INNER JOIN exchangerate t5 
		ON t5.Date = @QueryDate 
		AND t4.SourceCurrencyId = t5.SourceCurrencyId
		AND t5.DestinationCurrencyId = t0.CurrencyId
	INNER JOIN currency t6
		ON t4.SourceCurrencyId = t6.CurrencyId
GO


