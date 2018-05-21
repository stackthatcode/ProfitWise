use profitwise;





DROP VIEW IF EXISTS vw_profitwiseshop
GO

CREATE VIEW vw_profitwiseshop 
AS

SELECT t1.*, t2.LastStatus AS LastBillingStatus 
FROM profitwiseshop t1
LEFT JOIN profitwiserecurringcharge t2
ON t1.PwShopId = t2.PwShopId AND t2.IsPrimary = 1;
GO





DROP VIEW IF EXISTS [dbo].[vw_masterproductandvariantsearch]
GO

DROP VIEW IF EXISTS [dbo].[vw_standaloneproductandvariantsearch]
GO

DROP VIEW IF EXISTS [dbo].[vw_goodsonhand]
GO



-- TODO - this badly needs multi-tenant filtering (!!!)
CREATE VIEW [dbo].[vw_masterproductandvariantsearch] (
	[PwShopId], [PwMasterProductId], [ProductTitle], [Vendor], [ProductType], [PwMasterVariantId], [VariantTitle], [Sku])
AS 
   SELECT 
      t1.PwShopId AS PwShopId, 
      t1.PwMasterProductId AS PwMasterProductId, 
      t1.Title AS ProductTitle, 
      t1.Vendor AS Vendor, 
      t1.ProductType AS ProductType, 
      t3.PwMasterVariantId AS PwMasterVariantId, 
      t3.Title AS VariantTitle, 
      t3.Sku AS Sku
   FROM profitwiseproduct  AS t1 
      INNER JOIN profitwisemastervariant  AS t2 
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterProductId = t2.PwMasterProductId
      INNER JOIN profitwisevariant  AS t3 
		ON t2.PwShopId = t3.PwShopId AND t2.PwMasterVariantId = t3.PwMasterVariantId
   WHERE t1.IsPrimary = 1 AND t3.IsPrimary = 1 AND t2.Exclude = 0
GO

-- TODO - this badly needs multi-tenant filtering (!!!)
CREATE VIEW [dbo].[vw_standaloneproductandvariantsearch] (
	[PwShopId], [PwProductId], [ProductTitle], [Vendor], [ProductType], [PwVariantId], [VariantTitle], [Sku],
	[IsProductActive], [IsVariantActive], [Inventory], [PwMasterVariantId], [StockedDirectly])
AS 
   SELECT 
      t1.PwShopId AS PwShopId, 
      t1.PwProductId,
	  t1.Title AS ProductTitle, 
      t1.Vendor AS Vendor, 
      t1.ProductType AS ProductType, 
      t3.PwVariantId,
	  t3.Title AS VariantTitle, 
      t3.Sku AS Sku,
	  t1.IsActive AS IsProductActive,
	  t3.IsActive AS IsVariantActive,
	  t3.Inventory,
	  t3.[PwMasterVariantId],
	  t4.StockedDirectly
   FROM profitwiseproduct AS t1 
		INNER JOIN profitwisevariant AS t3
			ON t1.PwShopId = t3.PwShopId AND t1.PwProductId = t3.PwProductId
		INNER JOIN profitwisemastervariant AS t4
			ON t3.[PwMasterVariantId] = t4.[PwMasterVariantId]
GO


CREATE VIEW [dbo].[vw_goodsonhand]
AS 
	SELECT * FROM [vw_standaloneproductandvariantsearch]
	WHERE StockedDirectly = 1
	AND Inventory IS NOT NULL
	AND IsProductActive = 1
	AND IsVariantActive = 1
GO


IF OBJECT_ID (N'dbo.ufnNegToZero', N'FN') IS NOT NULL  
    DROP FUNCTION ufnNegToZero;  
GO  
CREATE FUNCTION dbo.ufnNegToZero(@input decimal(15, 2))  
RETURNS int   
AS   
BEGIN  
    DECLARE @output decimal(15, 2);  
    SET @output = 0;

    IF (@input IS NOT NULL AND @input > 0)   
        SET @output = @input;  

    RETURN @output;  
END;  
GO  





-- ### Multitenant Functions ###

DROP FUNCTION IF EXISTS dbo.batchstate
GO
CREATE FUNCTION dbo.batchstate(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisebatchstate WHERE PwShopId = @PwShopId;
GO

	
DROP FUNCTION IF EXISTS dbo.goodsonhandquerystub
GO
CREATE FUNCTION dbo.goodsonhandquerystub(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisegoodsonhandquerystub WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.masterproduct
GO
CREATE FUNCTION dbo.masterproduct(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisemasterproduct WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.mastervariant
GO
CREATE FUNCTION dbo.mastervariant(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisemastervariant WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.mastervariantcogscalc
GO
CREATE FUNCTION dbo.mastervariantcogscalc(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisemastervariantcogscalc WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.mastervariantcogsdetail
GO
CREATE FUNCTION dbo.mastervariantcogsdetail(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisemastervariantcogsdetail WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.picklist
GO
CREATE FUNCTION dbo.picklist(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisepicklist WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.picklistmasterproduct
GO
CREATE FUNCTION dbo.picklistmasterproduct(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisepicklistmasterproduct WHERE PwShopId = @PwShopId;
GO






DROP FUNCTION IF EXISTS dbo.product
GO
CREATE FUNCTION dbo.product(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwiseproduct WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.profitquerystub
GO
CREATE FUNCTION dbo.profitquerystub(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwiseprofitquerystub WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.profitreportentry
GO
CREATE FUNCTION dbo.profitreportentry(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwiseprofitreportentry WHERE PwShopId = @PwShopId;
GO

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




DROP FUNCTION IF EXISTS dbo.recurringcharge
GO
CREATE FUNCTION dbo.recurringcharge(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwiserecurringcharge WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.report
GO
CREATE FUNCTION dbo.report(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisereport WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.reportfilter
GO
CREATE FUNCTION dbo.reportfilter(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisereportfilter WHERE PwShopId = @PwShopId;
GO




DROP FUNCTION IF EXISTS dbo.tour
GO
CREATE FUNCTION dbo.tour(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisetour WHERE PwShopId = @PwShopId;
GO

DROP FUNCTION IF EXISTS dbo.shop
GO
CREATE FUNCTION dbo.shop(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwiseshop WHERE PwShopId = @PwShopId;
GO

DROP FUNCTION IF EXISTS dbo.variant
GO
CREATE FUNCTION dbo.variant(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisevariant WHERE PwShopId = @PwShopId;
GO


DROP FUNCTION IF EXISTS dbo.ordertable
GO
CREATE FUNCTION dbo.ordertable(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM shopifyorder WHERE PwShopId = @PwShopId;
GO

DROP FUNCTION IF EXISTS dbo.orderadjustment
GO
CREATE FUNCTION dbo.orderadjustment(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM shopifyorderadjustment WHERE PwShopId = @PwShopId;
GO

DROP FUNCTION IF EXISTS dbo.orderlineitem
GO
CREATE FUNCTION dbo.orderlineitem(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM shopifyorderlineitem WHERE PwShopId = @PwShopId;
GO

DROP FUNCTION IF EXISTS dbo.orderrefund
GO
CREATE FUNCTION dbo.orderrefund(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM shopifyorderrefund WHERE PwShopId = @PwShopId;
GO






-- Views
DROP FUNCTION IF EXISTS dbo.mtv_goodsonhand
GO
CREATE FUNCTION dbo.mtv_goodsonhand(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM vw_goodsonhand WHERE PwShopId = @PwShopId;
GO

DROP FUNCTION IF EXISTS dbo.mtv_masterproductandvariantsearch
GO
CREATE FUNCTION dbo.mtv_masterproductandvariantsearch(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM vw_masterproductandvariantsearch WHERE PwShopId = @PwShopId;
GO

DROP FUNCTION IF EXISTS dbo.mtv_standaloneproductandvariantsearch
GO
CREATE FUNCTION dbo.mtv_standaloneproductandvariantsearch(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM vw_standaloneproductandvariantsearch WHERE PwShopId = @PwShopId;
GO


