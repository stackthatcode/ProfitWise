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


SELECT EntryDate, SUM(NetSales), SUM(CoGS) FROM profitreportentryprocessed(100001, 1, 0.85, 1) 
WHERE EntryDate >= '2016-01-11'  AND EntryDate <= '2016-01-18'
GROUP BY EntryDate




