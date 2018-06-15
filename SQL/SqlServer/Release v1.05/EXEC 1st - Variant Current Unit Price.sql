USE ProfitWise;
GO



-- ALTER TABLE profitwisevariant DROP COLUMN CurrentPrice

IF COL_LENGTH('dbo.profitwisevariant', 'CurrentPrice') IS NULL
BEGIN
	ALTER TABLE profitwisevariant ADD CurrentPrice decimal(18, 2) NULL;
END
GO


UPDATE t1 
SET t1.CurrentPrice = t2.HighPrice
FROM profitwisevariant t1
	LEFT JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId
		AND t1.ShopifyVariantId = t2.ShopifyVariantId
		AND t2.IsActive = 1

--WHERE t1.PwShopId = 100001


-- *** FOR VERIFICATION PURPOSES

SELECT t1.PwShopId, t1.ShopifyVariantId, t1.PwVariantId, t2.PwVariantId, t1.CurrentPrice
FROM profitwisevariant t1
	LEFT JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId
		AND t1.ShopifyVariantId = t2.ShopifyVariantId
		AND t2.IsActive = 1

--WHERE t1.PwShopId = 100001


