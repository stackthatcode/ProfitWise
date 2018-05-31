
USE ProfitWise;
GO


SELECT t0.ShopifyVariantId, t0.PwVariantId, t1.Title AS ProductTile, t0.Title AS VariantTitle, Sku, 
		LowPrice, HighPrice, CurrentUnitPrice, MarginPercent / 100.0 AS MarginPercent, FixedAmount, Abbreviation
FROM dbo.costofgoodsbydate(100001, DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)) t0
	INNER JOIN dbo.product(100001) t1
		ON t0.PwProductId = t1.PwProductId
ORDER BY t1.Title, t0.Title, Sku


-- For example, this one has NULL CurrentUnitPrice - non existing product, but there's Order History
SELECT * FROM orderlineitem(100001) WHERE PwVariantId = 842;


