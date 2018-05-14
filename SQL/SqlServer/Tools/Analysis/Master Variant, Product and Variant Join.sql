USE ProfitWise
GO


-- For getting column names
SELECT * FROM product(100001) t1;

SELECT * FROM mastervariant(100001) t1;

SELECT * FROM variant(100001);



-- *** If everything is present, then this query works beautifully
SELECT t1.PwMasterVariantId AS ProfitWiseId, t2.Title, t3.Title, t3.Sku, t2.IsActive, t3.IsActive
FROM mastervariant(100001) t1
	INNER JOIN product(100001) t2
		ON t1.PwMasterProductId = t2.PwMasterProductId AND t2.IsPrimary = 1 AND t2.IsActive = 1
	INNER JOIN variant(100001) t3
		ON t1.PwMasterVariantId = t3.PwMasterVariantId AND t3.IsPrimary = 1 AND t3.IsActive = 1
WHERE t3.Sku = 'UM2PLUSMAINT';



SELECT t1.PwMasterVariantId,
	t2.Title, 
	t3.Title, 
	t3.Sku, 
	t2.IsActive AS IsProductActive, 
	t3.IsActive AS IsVariantActive
FROM profitwisemastervariant t1
	INNER JOIN profitwiseproduct t2
		ON t1.PwMasterProductId = t2.PwMasterProductId
		-- AND t2.IsPrimary = 1 AND t2.IsActive = 1
	INNER JOIN profitwisevariant t3
		ON t1.PwMasterVariantId = t3.PwMasterVariantId
		-- AND t3.IsPrimary = 1 AND t3.IsActive = 1
WHERE t3.Sku LIKE '%BUFFALOPLAIDCARD%';


SELECT * FROM mastervariant(100001)

SELECT * FROM profitwisemastervariant 
WHERE PwMasterVariantId NOT IN (SELECT PwMasterVariantId FROM profitwisemastervariantcogscalc)


