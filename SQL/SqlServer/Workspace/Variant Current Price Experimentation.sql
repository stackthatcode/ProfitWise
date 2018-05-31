USE ProfitWise;
GO


-- Any Active Variants (the most recently located in the Shopify Product catalog) 
-- ... should have same LowPrice and HighPrice .. thus zero rows

SELECT * FROM profitwisevariant 
WHERE LowPrice <> HighPrice AND IsActive = 1;

-- There are Variants that are Primary in the Master Variant with differing Low and High Prices

SELECT * FROM profitwisevariant
WHERE LowPrice <> HighPrice AND IsPrimary = 1;


-- This demonstrates that there are no Variants with more than one Active
SELECT PwShopId, ShopifyVariantId, COUNT(*) 
FROM profitwisevariant WHERE IsActive = 1 
GROUP BY PwShopId, ShopifyVariantId ORDER BY COUNT(*) DESC;




-- *** These are Variants for which there's no Active version

WITH CTE ( ShopId, ShopifyVariantId, VariantId, ActiveVariantId ) AS 
(
	SELECT t1.PwShopId, t1.ShopifyVariantId, t1.PwVariantId, t2.PwVariantId, t1.CurrentPrice
	FROM profitwisevariant t1
		LEFT JOIN profitwisevariant t2
			ON t1.PwShopId = t2.PwShopId
			AND t1.ShopifyVariantId = t2.ShopifyVariantId
			AND t2.IsActive = 1
	WHERE t1.PwShopId = 100001
) 
SELECT * FROM profitwisevariant 
WHERE ShopifyVariantId IN ( SELECT ShopifyVariantId FROM CTE WHERE ActiveVariantId IS NULL )
AND PwShopId = 100001
ORDER BY ShopifyVariantId;







-- ### SIDE ISSUE ###

-- *** Apparently 3D Universe's data contains Variants that should be flagged as Inactive for deletion

SELECT * FROM variant(100001) WHERE IsActive = 1 AND PwVariantId NOT IN ( SELECT PwVariantId FROM orderlineitem(100001) );

SELECT PwShopId, COUNT(*) FROM profitwiseproduct GROUP BY PwShopId;



