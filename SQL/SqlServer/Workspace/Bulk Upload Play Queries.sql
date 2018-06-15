
USE ProfitWise;
GO

DECLARE @PwShopId int = 100001;
DECLARE @Today datetime = DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0);

--SELECT * FROM dbo.costofgoodsbydate(100001, @Today) WHERE IsPrimary = 1;


SELECT t0.PwVariantId, t1.Title AS ProductTitle, t0.Title AS VariantTitle, Sku, 
		LowPrice, HighPrice, CurrentUnitPrice, MarginPercent / 100.0 AS MarginPercent, FixedAmount, Abbreviation
FROM dbo.costofgoodsbydate(@PwShopId, @Today) t0
	INNER JOIN dbo.product(@PwShopId) t1
		ON t0.PwProductId = t1.PwProductId
WHERE t0.IsPrimary = 1
ORDER BY t1.Title, t0.Title, Sku;






-- For example, this one has NULL CurrentUnitPrice - non existing product, but there's Order History
SELECT * FROM orderlineitem(100001) WHERE PwVariantId = 842;


