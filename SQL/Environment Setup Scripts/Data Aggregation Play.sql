USE ProfitWise;


SELECT * FROM shopifyorderlineitem;





SELECT DATE(t2.CreatedAt), t3.ReportedSku, SUM(t3.UnitPrice * t3.Quantity)
FROM shopifyorder t2 
	INNER JOIN shopifyorderlineitem t3
		ON t2.ShopifyOrderId = t3.ShopifyOrderLineId
/* WHERE t3.ReportedSku = 'UM2PLUS' */
GROUP BY DATE(t2.CreatedAt), t3.ReportedSku;

SELECT * FROM shopifyorder;
SELECT * FROM shopifyorderlineitem;



DROP VIEW TESTVIEW;

CREATE VIEW TESTVIEW
AS
SELECT t2.ShopId, DATE(t2.CreatedAt) AS CreatedAt, t3.ReportedSku, SUM(Quantity * UnitPrice) AS LineTotal
FROM shopifyorder t2 
	INNER JOIN shopifyorderlineitem t3
		ON t2.ShopId = t3.ShopId AND t2.ShopifyOrderId = t3.ShopifyOrderId
GROUP BY t2.ShopId, DATE(t2.CreatedAt), t3.ReportedSku;


SELECT * FROM TESTVIEW WHERE ShopId = 955973 AND ReportedSku LIKE 'UM2%' ORDER BY CreatedAt;

SELECT DISTINCT(ShopId) FROM shopifyorderlineitem


