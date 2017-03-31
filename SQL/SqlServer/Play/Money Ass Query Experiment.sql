 
DECLARE @PwShopId int = 100001;

WITH CTE_PriceSummary ( 
		PwMasterProductId, PwMasterVariantId, Exclude, StockedDirectly, CogsTypeId, CogsMarginPercent, 
		CogsCurrencyId, CogsAmount, CogsDetail, HighPriceAll, LowPriceAll, TotalInventory ) AS
(
	SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t2.Exclude, t2.StockedDirectly, t2.CogsTypeId, t2.CogsMarginPercent, 
				t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail, MAX(t3.HighPrice), MIN(t3.LowPrice), SUM(Inventory)
	FROM mastervariant(@PwShopId) t2 
		INNER JOIN variant(@PwShopId) t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
	WHERE t2.PwMasterProductId IN ( 
		@MasterProductIds1,@MasterProductIds2,@MasterProductIds3,@MasterProductIds4,@MasterProductIds5,@MasterProductIds6,@MasterProductIds7,@MasterProductIds8,@MasterProductIds9,@MasterProductIds10,@MasterProductIds11,@MasterProductIds12,@MasterProductIds13,@MasterProductIds14,@MasterProductIds15,@MasterProductIds16,@MasterProductIds17,@MasterProductIds18,@MasterProductIds19,@MasterProductIds20,@MasterProductIds21,@MasterProductIds22,@MasterProductIds23,@MasterProductIds24,@MasterProductIds25,@MasterProductIds26,@MasterProductIds27,@MasterProductIds28,@MasterProductIds29,@MasterProductIds30,@MasterProductIds31,@MasterProductIds32,@MasterProductIds33,@MasterProductIds34,@MasterProductIds35,@MasterProductIds36,@MasterProductIds37,@MasterProductIds38,@MasterProductIds39,@MasterProductIds40,@MasterProductIds41,@MasterProductIds42,@MasterProductIds43,@MasterProductIds44,@MasterProductIds45,@MasterProductIds46,@MasterProductIds47,@MasterProductIds48,@MasterProductIds49,@MasterProductIds50
	)
	GROUP BY t2.PwMasterProductId, t2.PwMasterVariantId, t2.Exclude, t2.StockedDirectly, t2.CogsTypeId, t2.CogsMarginPercent, 
			t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail
)
SELECT t1.PwMasterProductId, t1.PwMasterVariantId, t1.Exclude, t1.StockedDirectly, t1.CogsTypeId, t1.CogsMarginPercent, 
				t1.CogsCurrencyId, t1.CogsAmount, t1.CogsDetail, t1.HighPriceAll, t1.LowPriceAll, t1.TotalInventory,
				t3.Title, t3.Sku, t4.Title AS ProductTitle
FROM CTE_PriceSummary t1
	INNER JOIN variant(@PwShopId) t3 ON t1.PwMasterVariantId = t3.PwMasterVariantId
	    INNER JOIN product(@PwShopId) t4 ON t3.PwProductId = t4.PwProductId
WHERE t3.IsPrimary = 1;



--SELECT * FROM profitwisevariant



