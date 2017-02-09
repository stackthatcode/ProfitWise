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



SELECT * FROM profitwiseproduct WHERE PwMasterProductId = 219;


SELECT * FROM profitwisemastervariant WHERE PwMasterProductId = 219;
SELECT * FROM profitwisemastervariant WHERE PwMasterProductId = 220;



