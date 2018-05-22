USE ProfitWise;
GO

DECLARE @PwShopId int = 100001, @PwReportId int = 1, @EndDate datetime = '5/31/2018', @StartDate datetime = '1/1/2018';
DECLARE @UseDefaultMargin bit = 1, @DefaultCogsPercent decimal(9, 2) = 0.80, @MinPaymentStatus int = 1;


SELECT t1.Vendor, t1.ProductType, t1.ProductTitle, t1.VariantTitle, t1.Sku, 

SUM(t3.NetSales) As TotalRevenue, 
SUM(t3.Quantity) AS TotalQuantitySold, 
COUNT(DISTINCT(t3.OrderCountOrderId)) AS TotalOrders, 
CONVERT(decimal(18, 2), SUM(t3.CoGS)) AS TotalCogs, 
CONVERT(decimal(18, 2), SUM(t3.NetSales) - SUM(t3.CoGS)) AS TotalProfit, 
100.0 - dbo.SaveDivideAlt(100.0 * SUM(t3.CoGS), SUM(t3.NetSales), 100.0) AS AverageMargin,

dbo.SaveDivide(SUM(Quantity * UnitPrice), SUM(Quantity)) AS UnitPriceAverage, 
dbo.SaveDivide(SUM(Quantity * UnitCoGS), SUM(Quantity)) AS UnitCogsAverage, 
dbo.SaveDivide(SUM((UnitPrice - UnitCoGS) * Quantity), SUM(Quantity)) AS UnitMarginAverage, 

dbo.SaveDivide((SUM(NetSales) - SUM(CoGS)), SUM(Quantity)) AS AverageMarginPerUnitSold

FROM profitquerystub(@PwShopId) t1 
INNER JOIN variant(@PwShopId) t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId 
INNER JOIN profitreportentryprocessed(@PwShopId, @UseDefaultMargin, @DefaultCogsPercent, @MinPaymentStatus) t3 
	ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId AND t3.EntryDate >= @StartDate AND t3.EntryDate <= @EndDate 
	
WHERE t1.PwReportId = @PwReportId GROUP BY t1.Vendor, t1.ProductType, t1.ProductTitle, t1.VariantTitle, t1.Sku

--SELECT * FROM profitreportentry(100001);

