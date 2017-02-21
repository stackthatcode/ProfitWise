USE ProfitWise
GO


SELECT * FROM profitwisemastervariant;


DECLARE @Today DateTime;
SELECT @Today = DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)
SELECT * FROM exchangerate WHERE [Date] = @Today;




DECLARE @QueryDate DateTime = '2017-02-01';
DECLARE @PwShopId int = 100001;
DECLARE @PwReportId int = 2;

WITH Data_CTE ( PwProductId, PwVariantId, VariantTitle, Sku, Inventory, Price, CostOfGoodsOnHand, PotentialRevenue )
AS (
	SELECT	t2.PwProductId, t2.PwVariantId, t2.Title, t2.Sku, t2.Inventory, t2.HighPrice AS Price, 
			ISNULL(t2.Inventory * (t4.PercentMultiplier / 100 * t2.HighPrice + t4.FixedAmount * t5.Rate), 0) AS CostOfGoodsOnHand,
            ISNULL(t2.Inventory, 0) * t2.HighPrice AS PotentialRevenue
	FROM profitwisemastervariant t1
		INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId	
		LEFT JOIN profitwisemastervariantcogscalc t4 
			ON t1.PwShopId = t4.PwShopId AND t1.PwMasterVariantId = t4.PwMasterVariantId 
			AND t4.StartDate <= @QueryDate AND t4.EndDate > @QueryDate
		LEFT JOIN exchangerate t5 ON t4.SourceCurrencyId = t5.SourceCurrencyId
				AND t5.Date = @QueryDate AND t5.DestinationCurrencyId = 1 
	WHERE t1.PwShopId = @PwShopId
	AND t1.StockedDirectly = 1
	AND t2.PwVariantId IN ( 
		SELECT PwVariantId FROM profitwisegoodsonhandquerystub WHERE PwShopId = @PwShopId AND PwReportId = @PwReportId )
	AND t2.PwShopId = @PwShopId
	AND t2.Inventory IS NOT NULL
	AND t2.IsActive = 1 
) 
SELECT	t2.ProductType AS GroupingKey, t2.ProductType AS GroupingName, 
		MIN(Price) AS MinimumPrice,
		MAX(Price) AS MaximumPrice, 
		SUM(Inventory) AS TotalInventory,
		SUM(CostOfGoodsOnHand) AS TotalCostOfGoodsSold, 
		SUM(PotentialRevenue) AS TotalPotentialRevenue,	
		SUM(PotentialRevenue) - SUM(CostOfGoodsOnHand) AS TotalPotentialProfit
FROM Data_CTE t1
	INNER JOIN profitwiseproduct t2 ON t1.PwProductId = t2.PwProductId
WHERE t2.PwShopId = @PwShopId

GROUP BY t2.ProductType 

ORDER BY t2.ProductType ASC



SELECT * FROM [vw_standaloneproductandvariantsearch];

SELECT * FROM profitwisevariant WHERE Sku = 'VY5812'

SELECT * FROM profitwisemastervariant;


SELECT t1.PwProductId, t2.PwVariantId, t1.Vendor, t1.Title AS ProductTitle, 
                        t2.Title AS VariantTitle, t2.Sku
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisevariant t2 ON t1.PwProductId = t2.PwProductId
                    INNER JOIN profitwisemastervariant t3 
                        ON t2.PwMasterVariantId = t3.PwMasterVariantId
                WHERE t1.IsActive = 1
                AND t2.IsActive = 1 
                AND t2.Inventory IS NOT NULL
                AND t3.StockedDirectly = 1

-- ProductType, Vendor, PwProductId, PwVariantId, VariantTitle, Sku, Inventory, 
-- LowPrice, HighPrice, CostOfGoodsOnHand, PotentialRevenu


SELECT * FROM profitwisepicklistmasterproduct;


INSERT INTO profitwisepicklistmasterproduct (PwShopId, PwPickListId, PwMasterProductId)
SELECT DISTINCT t2.PwMasterProductId
FROM profitwisevariant t1
	 INNER JOIN profitwisemastervariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	 INNER JOIN profitwiseproduct t3 ON t2.PwMasterProductId = t3.PwMasterProductId
WHERE  ( (t1.Title LIKE 'UM3') OR (t1.Sku LIKE 'UM3') OR ( t3.Title LIKE 'UM3' ) OR ( t3.Vendor LIKE 'UM3' ) )


