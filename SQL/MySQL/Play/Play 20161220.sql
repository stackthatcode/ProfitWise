USE profitwise;






## SAVE *** Update all the CoGS based on 20% margin
UPDATE profitwiseshop t0
    INNER JOIN profitwisemastervariant t1 
		ON t0.PwShopId = t1.PwShopId
	INNER JOIN profitwisevariant t2 
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId AND t2.IsPrimary = 1
SET t1.CogsCurrencyId = t0.CurrencyId, t1.CogsAmount = t2.HighPrice * 0.80, t1.CogsDetail = false
WHERE t0.PwShopId = 100001
AND t1.CogsAmount IS NULL;


UPDATE profitwisemastervariant SET CoGSAmount = NULL;

#$1000 @ 20% Margin = $800
#$800 @ 20% ROI => $960

UPDATE profitwisemastervariant SET CurrencyId = 1, CogsAmount = 0 WHERE CogsAmount = NULL


## SAVE *** Update all the Order Line CoGS
UPDATE profitwiseshop t0
    INNER JOIN profitwisemastervariant t1 
		ON t0.PwShopId = t1.PwShopId
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	LEFT JOIN exchangerate t4
		ON Date(t3.OrderDate) = t4.`Date` 
			AND t4.SourceCurrencyId = t1.CogsCurrencyId
			AND t4.DestinationCurrencyId = t0.CurrencyId
SET t3.UnitCogs = (t1.CogsAmount * IFNULL(t4.Rate, 0))
WHERE t0.PwShopId = 100001
AND t3.UnitCogs IS NULL;



UPDATE shopifyorderlineitem SET UnitCogs = NULL;

SELECT * FROM shopifyorderlineitem;



### Using the Query Stub, we can isolate Order Lines

SET @rank=0;
SELECT * FROM profitwisereportquerystub;



SELECT 	SUM(t3.GrossRevenue) As TotalRevenue, 
        SUM(t3.Quantity - t3.TotalRestockedQuantity) AS TotalNumberSold,
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereport t0
	INNER JOIN profitwisereportquerystub t1
		ON t0.PwShopId = t1.PwShopId AND t0.PwReportId = t1.PwReportId 
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  
			AND t3.OrderDate >= t0.StartDate AND t3.OrderDate <= t0.EndDate   
WHERE t0.PwShopId = 100001 AND t0.PwReportID = 99739;
          



SELECT 	'Product' AS GroupingType, t1.PwMasterProductId AS GroupingId, t1.ProductTitle AS GroupingName,
		SUM(t3.GrossRevenue) As TotalRevenue, 
        SUM(t3.Quantity - t3.TotalRestockedQuantity) AS TotalNumberSold,
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereport t0
	INNER JOIN profitwisereportquerystub t1
		ON t0.PwShopId = t1.PwShopId AND t0.PwReportId = t1.PwReportId 
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  
			AND t3.OrderDate >= t0.StartDate AND t3.OrderDate <= t0.EndDate             
WHERE t0.PwShopId = 100001 AND t0.PwReportID = 99739
GROUP BY t1.PwMasterProductId, t1.ProductTitle
ORDER BY TotalRevenue DESC
LIMIT 10;







SELECT * FROM calendar_table;

### Report Series Generation
### { y + y, y + q, y + m, w, dw }


SELECT  t4.y, t4.m, CONCAT(CONCAT(t4.monthName, ' '),  t4.y) AS DateLabel, 
		t1.ProductTitle AS GroupingName, 	# Grouping 
		SUM(t3.GrossRevenue) AS TotalRevenue, 
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	INNER JOIN calendar_table t4
		ON t3.OrderDate = t4.dt
WHERE t1.PwShopId = 100001 AND t1.PwReportID = 99739 AND t3.OrderDate >= '2014-01-02'

# AND t1.ProductTitle IN ( 'Ultimaker', 'ColorFabb' )

GROUP BY t4.y, t4.m, DateLabel, GroupingName # Build Query Tail ( Granularity )
ORDER BY t4.y, t4.m;



SELECT t4.y, t4.m, t4.monthName AS DateLabel, 
		t1.Vendor AS GroupingName, 	# Grouping 
		SUM(t3.GrossRevenue) AS TotalRevenue, 
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	INNER JOIN calendar_table t4
		ON t3.OrderDate = t4.dt
WHERE t1.PwShopId = 100001 AND t1.PwReportID = 99739 AND t3.OrderDate >= '2014-01-02'

AND t1.Vendor IN ( 'Ultimaker', 'ColorFabb' )

GROUP BY t4.y, t4.m, DateLabel, GroupingName 
ORDER BY t4.y, t4.m;


SELECT * FROM profitwisemastervariant;



SELECT t3.OrderDate,
		SUM(t3.GrossRevenue) AS TotalRevenue, 
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
WHERE t1.PwShopId = 100001 AND t1.PwReportID = 99739 
GROUP BY t3.OrderDate
ORDER BY t3.OrderDate


SELECT * FROM profitwisereportquerystub;

SELECT * FROM calendar_table WHERE y = 2016;

SELECT * FROM profitwisevariant WHERE PwPRoductId IN ( '1', '2' );

SELECT * FROM profitwisereport;


## Other stuff
SELECT * FROM profitwiseshop;

UPDATE profitwiseshop SET StartingDateForOrders = '2014-01-01';

SELECT * FROM profitwisemastervariant;

SELECT COUNT(*) FROM shopifyorderlineitem;

SELECT * FROM exchangerate;

SELECT * FROM calendar_table WHERE y = 2016;



SELECT * FROM profitwisereport;


SELECT COUNT(DISTINCT(t1.PwMasterProductId))
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  
			AND t3.OrderDate >= '2016-01-01' AND t3.OrderDate <= '2016-12-31'  
WHERE t1.PwShopId = 100001 AND t1.PwReportId = 99756                             

SELECT COUNT(DISTINCT(t1.ProductType))
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  
			AND t3.OrderDate >= '2016-01-01' AND t3.OrderDate <= '2016-12-31'  
WHERE t1.PwShopId = 100001 AND t1.PwReportId = 99756                             





SELECT t1.PwMasterVariantId AS GroupingKey,
 CONCAT(t1.Sku, ' - ', t1.VariantTitle) AS GroupingName, SUM(t3.GrossRevenue) As TotalRevenue, 
 SUM(t3.Quantity - t3.TotalRestockedQuantity) AS TotalNumberSold,
 SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs,
 SUM(t3.GrossRevenue) - SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalProfit,
 SUM(t3.GrossRevenue) - SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) /
 SUM(t3.Quantity - t3.TotalRestockedQuantity) AS AverageMargin
 FROM profitwisereportquerystub t1
 INNER JOIN profitwisevariant t2
 ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
 INNER JOIN shopifyorderlineitem t3
 ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
 AND t3.OrderDate >= @StartDate AND t3.OrderDate <= @EndDate 
 WHERE t1.PwShopId = @PwShopId AND t1.PwReportId = @PwReportId 
 GROUP BY t1.PwMasterVariantId, GroupingName 
 ORDER BY TotalProfit DESC LIMIT @StartingIndex, @PageSize
 





