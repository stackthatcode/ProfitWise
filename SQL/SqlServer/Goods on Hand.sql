USE ProfitWise
GO


DROP VIEW IF EXISTS [dbo].[vw_EffectiveCogsDetail]
GO

CREATE VIEW vw_EffectiveCogsDetail
AS

WITH MasterVariantMaxCogsDate ( PwShopId, PwMasterVariantId, MaxCogsDate ) 
AS
(
	SELECT PwShopId, PwMasterVariantId, MAX(CogsDate) AS MaxCogsDate
	FROM profitwisemastervariantcogsdetail t1
	WHERE CogsDate <= getdate()	
	GROUP BY PwShopId, PwMasterVariantId
)
SELECT t2.* 
FROM MasterVariantMaxCogsDate t1 
	INNER JOIN profitwisemastervariantcogsdetail t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId AND t1.MaxCogsDate = t2.CogsDate
GO




-- Margin Percent % and Fixed Amount => We'll call this the Goods on Hand Base Query

DROP VIEW IF EXISTS [dbo].[vw_GoodsOnHandBaseQuery]
GO

CREATE VIEW vw_GoodsOnHandBaseQuery
AS



SELECT t2.PwMasterVariantId
FROM profitwisepicklistmasterproduct t1  
	INNER JOIN profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId

GO

SELECT * FROM profitwisemastervariantcogscalc;

FROM profitwisereportquerystub t1
	INNER JOIN [vw_GoodsOnHandBaseQuery] t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	LEFT OUTER JOIN [vw_EffectiveCogsDetail] t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
WHERE t1.PwShopId = 100001 
AND t1.PwReportId = 1
AND t2.PwShopId = 100001



SELECT * FROM [vw_EffectiveCogsDetail]

SELECT * FROM profitwisereportquerystub;
