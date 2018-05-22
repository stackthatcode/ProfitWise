USE ProfitWise;
GO

DECLARE @PwShopId int = 100001;
DECLARE @Start datetime = '5/1/2018';
DECLARE @End datetime = '5/31/2018';



SELECT SUM(NetSales)
FROM dbo.profitreportentry(@PwShopId)
WHERE EntryDate >= @Start AND EntryDate <= @End

/*
DECLARE @PwShopId int = 100001;
DECLARE @Start datetime = '5/10/2018';
DECLARE @End datetime = '5/10/2018';

SELECT EntryDate, ShopifyOrderId, SUM(NetSales)
FROM dbo.profitreportentry(@PwShopId)
WHERE EntryDate >= @Start AND EntryDate <= @End
GROUP BY EntryDate, ShopifyOrderId ORDER BY EntryDate, SUM(NetSales) ASC
*/

SELECT EntryDate, SUM(NetSales)
FROM dbo.profitreportentry(@PwShopId)
WHERE EntryDate >= @Start AND EntryDate <= @End
GROUP BY EntryDate ORDER BY EntryDate

/*

DECLARE @PwShop int = 100087;
DECLARE @ShopifyOrderId bigint = 356888805425;

SELECT * FROM ordertable(@PwShop) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM orderlineitem(@PwShop) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM profitreportentry(@PwShop) WHERE ShopifyOrderId = @ShopifyOrderId;

*/

DELETE FROM profitreportentry(100001) WHERE ShopifyOrderId IN 
( SELECT ShopifyOrderId FROM ordertable(100001) WHERE Cancelled = 1 );


