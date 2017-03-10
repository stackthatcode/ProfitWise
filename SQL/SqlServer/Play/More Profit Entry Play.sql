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




SELECT * FROM dbo.profitreportentry(100001);



SELECT SUM(NetSales) FROM dbo.profitreportentryprocessed(100001, 1, 0.85, 1) 
WHERE EntryDate = '2016-01-11' AND EntryType = 3;


SELECT SUM(NetSales) FROM dbo.profitreportentryprocessed(100001, 1, 0.80, 1) 
WHERE EntryDate >= '2016-01-01' AND EntryDate <= '2016-12-31' 



-- WHERE EntryDate = '2016-01-11' ORDER BY ShopifyOrderId

DECLARE @PwShopId int = 100001;

SELECT t1.PwMasterProductId AS GroupingKey, t1.ProductTitle AS GroupingName, 
	SUM(t3.NetSales) As TotalRevenue,
	SUM(t3.Quantity) AS TotalQuantitySold,
	COUNT(DISTINCT(t3.ShopifyOrderId)) AS TotalOrders,        
	SUM(t3.CoGS) AS TotalCogs,                 
	SUM(t3.NetSales) - SUM(t3.CoGS) AS TotalProfit,                
	CASE WHEN SUM(t3.NetSales) = 0 THEN 0                     
	ELSE 100.0 - (100.0 * SUM(t3.CoGS) / SUM(t3.NetSales)) END AS AverageMargin 
FROM profitquerystub(@PwShopId) t1                
	INNER JOIN variant(@PwShopId) t2                    
		ON t1.PwMasterVariantId = t2.PwMasterVariantId                     
	INNER JOIN profitreportentryprocessed(@PwShopId, 1, 0.80, 1) t3                    
		ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId 
			AND t3.EntryDate >= '2016-01-01' AND t3.EntryDate <='2016-12-31'   
WHERE t1.PwMasterProductId = 68
GROUP BY t1.PwMasterProductId, t1.ProductTitle 


ORDER BY TotalProfit DESC OFFSET 1 ROWS FETCH NEXT 10 ROWS ONLY;

