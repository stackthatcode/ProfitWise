
USE ProfitWise;
GO


SELECT * FROM variant(100056) WHERE PwMasterVariantId NOT IN
( SELECT DISTINCT PwMasterVariantId FROM profitwisevariant WHERE IsPrimary = 1 );

UPDATE variant(100056) SET IsPrimary = 1 WHERE PwMasterVariantId NOT IN
( SELECT DISTINCT PwMasterVariantId FROM profitwisevariant WHERE IsPrimary = 1 );



SELECT * FROM variant(100066) WHERE PwMasterVariantId NOT IN
( SELECT DISTINCT PwMasterVariantId FROM variant(100066) WHERE IsPrimary = 1 );

SELECT * FROM product(100066) WHERE PwProductId IN ( 
SELECT PwProductId FROM variant(100066) WHERE PwMasterVariantId NOT IN
( SELECT DISTINCT PwMasterVariantId FROM variant(100066) WHERE IsPrimary = 1 ) );

UPDATE variant(100066) SET IsPrimary = 1  WHERE PwMasterVariantId NOT IN
( SELECT DISTINCT PwMasterVariantId FROM variant(100066) WHERE IsPrimary = 1 );





SELECT * FROM mastervariant(100001)



SELECT t1.PwMasterVariantId,
	t2.Title, 
	t3.Title, 
	t3.Sku, 
	t2.IsActive AS IsProductActive, 
	t3.IsActive AS IsVariantActive
FROM mastervariant(100001) t1
	INNER JOIN product(100001) t2
		ON t1.PwMasterProductId = t2.PwMasterProductId
		-- AND t2.IsPrimary = 1 AND t2.IsActive = 1
	INNER JOIN variant(100001) t3
		ON t1.PwMasterVariantId = t3.PwMasterVariantId
		-- AND t3.IsPrimary = 1 AND t3.IsActive = 1
WHERE t3.PwVariantId = 




SELECT * FROM orderlineitem(100001) WHERE PwVariantId IN 
( SELECT PwVariantId FROM variant(100001) WHERE PwMasterVariantId NOT IN
( SELECT DISTINCT PwMasterVariantId FROM variant(100001) WHERE IsPrimary = 1 ) );



SELECT PwMasterVariantId, COUNT(*) FROM variant(100001) GROUP BY PwMasterVariantId;



SELECT * FROM masterproduct(100001) WHERE PwMasterProductId = 73;

