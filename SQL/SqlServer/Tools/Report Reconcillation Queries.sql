USE ProfitWise;
GO

DECLARE @PwShopId int = 100001;
DECLARE @Start datetime = '4/1/2018';
DECLARE @End datetime = '4/19/2018';

SELECT *
FROM dbo.profitreportentry(@PwShopId)
WHERE EntryDate >= @Start AND EntryDate <= @End
GROUP BY EntryDate, ShopifyOrderId ORDER BY EntryDat


SELECT EntryType, COUNT(DISTINCT(OrderCountOrderId))
FROM dbo.profitreportentry(100001)
WHERE EntryDate >= @Start AND EntryDate <= @End
GROUP BY EntryType

GROUP BY ShopifyOrderId, OrderCountOrderId
ORDER BY ShopifyOrderId;



SELECT EntryDate, ShopifyOrderId, SUM(NetSales)
FROM dbo.profitreportentry(@PwShopId)
WHERE EntryDate >= @Start AND EntryDate <= @End
GROUP BY EntryDate, ShopifyOrderId ORDER BY EntryDate


SELECT EntryDate, SUM(NetSales)
FROM dbo.profitreportentry(@PwShopId)
WHERE EntryDate >= @Start AND EntryDate <= @End
GROUP BY EntryDate ORDER BY EntryDate



DECLARE @PwShop int = 100087;
DECLARE @ShopifyOrderId bigint = 356888805425;

SELECT * FROM ordertable(@PwShop) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM orderlineitem(@PwShop) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM profitreportentry(@PwShop) WHERE ShopifyOrderId = @ShopifyOrderId;

