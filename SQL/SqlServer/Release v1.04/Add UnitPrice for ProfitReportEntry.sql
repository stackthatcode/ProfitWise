USE ProfitWise;
GO

IF COL_LENGTH('dbo.profitwiseprofitreportentry', 'UnitCoGS') IS NOT NULL
BEGIN
	ALTER TABLE profitwiseprofitreportentry DROP COLUMN UnitCoGS
END

IF COL_LENGTH('dbo.profitwiseprofitreportentry', 'UnitPrice') IS NOT NULL
BEGIN
	ALTER TABLE profitwiseprofitreportentry DROP COLUMN UnitPrice
END

ALTER TABLE profitwiseprofitreportentry ADD UnitPrice decimal(18, 2) NULL;

ALTER TABLE profitwiseprofitreportentry ADD UnitCoGS decimal(18, 2) NULL;


ALTER TABLE dbo.shopifyorderlineitem ALTER COLUMN Quantity int;


-- This serves the Inventory Valuation Report query
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_ProfitReportEntry_UpdateIndex')   
    DROP INDEX IX_ProfitReportEntry_UpdateIndex ON profitwiseprofitreportentry;   
GO  

ALTER TABLE dbo.profitwiseprofitreportentry ALTER COLUMN Quantity DECIMAL(18,2);

CREATE NONCLUSTERED INDEX IX_ProfitReportEntry_UpdateIndex   
    ON profitwiseprofitreportentry (PwShopId) INCLUDE ( EntryDate, EntryType, ShopifyOrderId, SourceId, Quantity )
GO 



DROP FUNCTION IF EXISTS dbo.SaveDivide
GO
CREATE FUNCTION dbo.SaveDivide (@Dividend decimal(18, 2), @Divisor decimal(18, 2))
	RETURNS decimal(18, 2)
AS
BEGIN
	RETURN CASE WHEN @Divisor = 0 THEN 0 ELSE @Dividend / @Divisor END
END
GO

DROP FUNCTION IF EXISTS dbo.SaveDivideAlt
GO
CREATE FUNCTION dbo.SaveDivideAlt (@Dividend decimal(18, 2), @Divisor decimal(18, 2), @AlternateValue decimal(18, 2))
	RETURNS decimal(18, 2)
AS
BEGIN
	RETURN CASE WHEN @Divisor = 0 THEN @AlternateValue ELSE @Dividend / @Divisor END
END
GO



DROP INDEX [exchangerate].[IX_exchangerate_date]
GO

CREATE NONCLUSTERED INDEX [IX_exchangerate_date] ON [dbo].[exchangerate] ([Date])
INCLUDE ([SourceCurrencyId],[DestinationCurrencyId],[Rate])



DROP FUNCTION IF EXISTS dbo.costofgoodsbydate
GO

CREATE FUNCTION dbo.costofgoodsbydate(@PwShopId bigint, @QueryDate datetime)  
RETURNS TABLE  
AS  
RETURN
SELECT	t2.PwProductId, 
		t2.PwVariantId, 
		t2.Title, 
		t2.Sku, 
		dbo.ufnNegToZero(t2.Inventory) AS Inventory, 
		t1.StockedDirectly,
		t1.Exclude,
		t2.IsActive,
		t2.HighPrice AS CurrentUnitPrice,
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
GO



--SELECT * FROM dbo.costofgoodsbydate(100001, '05/23/2018');





