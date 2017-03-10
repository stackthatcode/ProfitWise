USE ProfitWise
GO


SELECT * FROM shopifyorderlineitem;

SELECT * FROM profitreportentry(100001) WHERE EntryType = 2;


SELECT *	 FROM dbo.profitreportentry(100001) 



DECLARE @PwShopId int = 100001;
DECLARE @UseDefaultMargin tinyint = 1;
DECLARE @DefaultCogsPercent decimal(15, 2) = .85;	-- Remember, this is Cogs Percent, not Margin/Profit


SELECT t2.PwShopId, EntryDate, EntryType, ShopifyOrderId, SourceId, PwProductId, PwVariantId, NetSales, CoGS, Quantity, PaymentCleared
FROM dbo.profitreportentry(100001) t2
		ON t1.PwShopId = t2.PwShopId;


SELECT * FROM dbo.profitreportentry(100001) WHERE PaymentCleared > -1



DROP FUNCTION IF EXISTS dbo.profitreportentryprocessed
GO
CREATE FUNCTION dbo.profitreportentryprocessed (
		@PwShopId bigint, @UseDefaultMargin tinyint, @DefaultCogsPercent decimal(15, 2), @MinPaymentStatus tinyint)  
RETURNS TABLE
AS  
RETURN SELECT PwShopId, EntryDate, EntryType, ShopifyOrderId, SourceId, PwProductId, PwVariantId, NetSales, 
	CASE WHEN (@UseDefaultMargin = 1 AND ISNULL(CoGS, 0) = 0) THEN NetSales * @DefaultCoGSPercent ELSE CoGS END AS CoGS, 
	Quantity, PaymentStatus
FROM dbo.profitreportentry(@PwShopId)
WHERE PaymentStatus >= @MinPaymentStatus
GO


SELECT * FROM dbo.profitreportentryprocessed(100001, 1, 0.85, 1);



