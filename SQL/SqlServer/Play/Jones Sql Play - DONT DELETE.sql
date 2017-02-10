USE ProfitWise
GO


use master
ALTER DATABASE ProfitWise SET SINGLE_USER WITH ROLLBACK IMMEDIATE 

--do you stuff here 

ALTER DATABASE ProfitWise SET MULTI_USER


SELECT * FROM profitwiseshop;

SELECT * FROM profitwiseproduct;
SELECT * FROM profitwiseproduct
WHERE PwShopId = 100001 AND PwMasterProductId = 220;

SELECT * FROM profitwisemastervariant;

SELECT * FROM profitwisevariant;

SELECT * FROM profitwisemastervariantcogsdetail;

SELECT t1.* 
FROM profitwisemastervariantcogsdetail t1
	INNER JOIN profitwisemastervariant t2 
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId



USE ProfitWise
GO

SELECT * FROM profitwiseproduct WHERE PwMasterProductId IN ( 222, 223 );

SELECT * FROM profitwisemastervariant WHERE PwMasterProductId = 222;

SELECT * FROM profitwisevariant WHERE PwMasterVariantId IN ( SELECT PwMasterVariantId FROM profitwisemastervariant WHERE PwMasterProductId = 222 );

SELECT * FROM profitwisevariant WHERE SKU Like '3DUPLA285%';


SELECT t1.PwMasterVariantId, t2.Sku, t2.Title AS VariantTitle, t3.Title AS ProductTitle
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId AND t2.IsPrimary = 1
	INNER JOIN profitwiseproduct t3 ON t2.PwProductId = t3.PwProductId
WHERE t1.PwShopId = 100001
AND t2.PwShopId = 100001
AND t3.PwShopId = 100001
AND t1.PwMasterProductId = 150 



