USE ProfitWise
GO


SELECT * FROM profitreportentry(100001)

SELECT t1.ShopifyOrderID, COUNT(t1.SourceId)
FROM profitreportentry(100001) t1
	INNER JOIN ordertable(100001) t2
		ON t1.ShopifyOrderId = t2.ShopifyOrderId
GROUP BY t1.ShopifyOrderID

SELECT * FROM ordertable(100001) WHERE OrderNumber = '#5238072';

SELECT * FROM ordertable(100001) WHERE OrderDate = '2017-01-01';

SELECT * FROM profitreportentry(100001) WHERE ShopifyOrderId = 4460774162;

SELECT * FROM report(100001);

SELECT * FROM shop(100001);

SELECT * FROM product(100001) WHERE Title LIKE '%Ultimaker 2%'

SELECT * FROM variant(100001) WHERE SKU LIKE '%UM2%'

SELECT * FROM batchstate(100001);



