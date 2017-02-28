use profitwise;

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


DROP FUNCTION IF EXISTS dbo.reportquerystub
GO
CREATE FUNCTION dbo.reportquerystub(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisereportquerystub WHERE PwShopId = @PwShopId;
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


