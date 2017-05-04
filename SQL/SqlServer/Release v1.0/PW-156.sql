USE ProfitWise
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'profitwiseprofitreportentry' AND COLUMN_NAME = 'OrderCountOrderId')
BEGIN
    ALTER TABLE [dbo].[profitwiseprofitreportentry] ADD 
        [OrderCountOrderId] bigint NULL DEFAULT NULL
		
END



DROP FUNCTION IF EXISTS dbo.profitreportentryprocessed
GO
CREATE FUNCTION dbo.profitreportentryprocessed (
		@PwShopId bigint, @UseDefaultMargin tinyint, @DefaultCogsPercent decimal(15, 2), @MinPaymentStatus tinyint)  
RETURNS TABLE
AS  
RETURN SELECT PwShopId, EntryDate, EntryType, ShopifyOrderId, SourceId, PwProductId, PwVariantId, NetSales, 
	CASE WHEN (@UseDefaultMargin = 1 AND ISNULL(CoGS, 0) = 0) THEN NetSales * @DefaultCoGSPercent ELSE CoGS END AS CoGS, 
	Quantity, PaymentStatus, OrderCountOrderId
FROM dbo.profitreportentry(@PwShopId)
WHERE PaymentStatus >= @MinPaymentStatus
GO



-- #### IMPORTANT - DON'T FORGET TO RUN THE VIEWS AND FUNCTIONS DDL ####



