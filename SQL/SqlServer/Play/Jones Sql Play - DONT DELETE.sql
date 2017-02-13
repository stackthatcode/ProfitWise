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

SELECT * FROM profitwisevariant 
WHERE PwMasterVariantId IN ( 
	SELECT PwMasterVariantId FROM profitwisemastervariant WHERE PwMasterProductId = 222 
);

SELECT * FROM profitwisevariant WHERE SKU Like '3DUPLA285%';

SELECT * FROM profitwisereportquerystub;

SELECT * FROM profitwiseprofitreportentry;



SELECT * FROM profitwisemastervariant;

SELECT * FROM profitwisevariant;



-- Fixed Amount
SELECT t1.PwMasterVariantId, t2.Inventory, t1.CogsAmount
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
WHERE t1.StockedDirectly = 1
-- AND t2.IsActive = 1 
AND t2.Inventory IS NOT NULL
AND t1.CogsTypeId = 1
AND t1.CogsDetail = 0;





-- Margin Percent % and Fixed Amount
SELECT t2.PwVariantId, t2.PwProductId, t2.Inventory, t1.CogsDetail, t1.CogsTypeId, t1.CogsCurrencyId, t1.CogsAmount, t1.CogsMarginPercent, t2.LowPrice, t2.HighPrice
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN profitwiseproduct t3 ON t2.PwProductId = t3.PwProductId
WHERE t1.StockedDirectly = 1
AND t2.Inventory IS NOT NULL
--AND t1.IsActive = 1 


-- Need to extract maximum CoGS Detail before today's Date
SELECT PwMasterVariantId, MAX(CogsDate), CogsTypeId, CogsCurrencyId, CogsAmount, CogsMarginPercent
FROM profitwisemastervariantcogsdetail
WHERE CogsDate < getdate()
GROUP BY PwMasterVariantId, CogsTypeId, CogsCurrencyId, CogsAmount, CogsMarginPercent;



SELECT * FROM profitwisemastervariant WHERE CogsDetail = 1;

SELECT * FROM profitwisemastervariantcogsdetail;


