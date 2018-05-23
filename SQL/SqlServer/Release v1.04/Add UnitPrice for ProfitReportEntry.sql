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


ALTER TABLE dbo.shopifyorderlineitem ALTER COLUMN Quantity DECIMAL(18,2);


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



DROP FUNCTION IF EXISTS dbo.inventoryvaluebydate
GO
CREATE FUNCTION dbo.inventoryvaluebydate(@PwShopId bigint, @QueryDate datetime)  
RETURNS TABLE  
AS  
RETURN
SELECT	t2.PwProductId, 
		t2.PwVariantId, 
		t2.Title, 
		t2.Sku, 
		dbo.ufnNegToZero(t2.Inventory) AS Inventory, 
		t2.HighPrice AS Price, 
        CASE WHEN (t4.PercentMultiplier = 0 AND t4.FixedAmount = 0 AND DefaultMargin <> 0) THEN (DefaultMargin / 100.0) * t2.HighPrice * dbo.ufnNegToZero(t2.Inventory)
        ELSE (ISNULL(t4.PercentMultiplier, 0) / 100 * t2.HighPrice + ISNULL(t4.FixedAmount, 0) * t5.Rate) * dbo.ufnNegToZero(t2.Inventory) END AS CostOfGoodsOnHand,
        dbo.ufnNegToZero(t2.Inventory) * t2.HighPrice AS PotentialRevenue
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
    ON t4.SourceCurrencyId = t5.SourceCurrencyId
		AND t5.Date = @QueryDate 
		AND t5.DestinationCurrencyId = t0.CurrencyId
WHERE t1.StockedDirectly = 1
AND t2.Inventory IS NOT NULL
AND t2.IsActive = 1 
GO


DECLARE @PwShopId int = 100001, @PwReportId int = 99821;
DECLARE @QueryDate datetime = '05/23/2018';


SELECT * FROM dbo.inventoryvaluebydate(@PwShopId, @QueryDate) 
WHERE PwVariantId IN ( SELECT PwVariantId FROM goodsonhandquerystub(@PwShopId) WHERE PwReportId = @PwReportId )





