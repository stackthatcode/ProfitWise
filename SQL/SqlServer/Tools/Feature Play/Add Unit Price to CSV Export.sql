

SELECT * FROM profitreportentry(100001) WHERE PwVariantId = 1915;

SELECT * FROM mastervariantcogscalc(100001);

SELECT * FROM variant(100001) WHERE Sku = '3DUPLA285BLACK750';

SELECT * FROM report(100001);




DECLARE @PwShopId int = 100001, @PwReportId int = 99821;
DECLARE @QueryDate datetime = '05/23/2018';

SELECT PwProductId, PwVariantId, Title, Sku, Inventory, CurrentUnitPrice, UnitCogsByDate, Inventory * UnitCogsByDate AS PotentialRevenue
FROM dbo.costofgoodsbydate(@PwShopId, @QueryDate) 
WHERE PwVariantId IN ( SELECT PwVariantId FROM goodsonhandquerystub(@PwShopId) WHERE PwReportId = @PwReportId )
