USE profitwise;






## SAVE *** Update all the CoGS to 
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
WHERE t0.PwShopId = 100001;



SELECT * FROM shopifyorderlineitem;



### Using the Query Stub, we can isolate Order Lines

SET @rank=0;
SELECT * FROM profitwisereportquerystub;



SELECT 	t1.PwMasterVariantId,
		SUM(t3.GrossRevenue) As TotalRevenue, 
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
WHERE t1.PwShopId = 100001 AND t1.PwReportID = 99742 AND t3.OrderDate >= '2013-11-01'
GROUP BY t1.PwMasterVariantId
ORDER BY TotalRevenue DESC;



SELECT COUNT(*) FROM shopifyorderlineitem INNER JOIN
ints;



SELECT 	t1.PwMasterVariantId,
		SUM(t3.GrossRevenue) As TotalRevenue, 
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
WHERE t1.PwShopId = 100001 AND t1.PwReportID = 99742 AND t3.OrderDate >= '2013-11-01'
GROUP BY t1.PwMasterVariantId
ORDER BY TotalRevenue DESC
LIMIT 10, 500000;



SELECT 	t1.Vendor,
		SUM(t3.GrossRevenue) AS TotalRevenue, 
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	INNER JOIN ints ON ints.i < 5
WHERE t1.PwShopId = 100001 AND t1.PwReportID = 99742 AND t3.OrderDate >= '2013-11-02'
GROUP BY t1.Vendor
ORDER BY TotalRevenue DESC
LIMIT 10;





### Report Series Generation
### { y + y, y + q, y + m, w, dw }

SELECT 	t4.y, t4.m, t1.Vendor, 
		SUM(t3.GrossRevenue), 
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	INNER JOIN calendar_table t4
		ON t3.OrderDate = t4.dt
WHERE t1.PwShopId = 100001 AND t1.PwReportID = 99740 AND t3.OrderDate >= '2014-01-02'
AND t1.Vendor IN ( 'Ultimaker', 'ColorFabb' )
GROUP BY t1.Vendor, t4.y, t4.m
ORDER BY t4.y, t4.m, t1.Vendor;




SELECT 	t1.PwMasterProductId,
		SUM(t3.GrossRevenue) AS TotalRevenue, 
		SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	INNER JOIN calendar_table t4
		ON t3.OrderDate = t4.dt
WHERE t1.PwShopId = 100001 
AND t1.PwReportID = 99740 
AND t3.OrderDate >= '2015-01-02'
##AND t1.Vendor = 'LulzBot'
GROUP BY t1.PwMasterProductId
ORDER BY TotalCogs DESC;





## Other stuff
SELECT * FROM profitwiseshop;

UPDATE profitwiseshop SET StartingDateForOrders = '2014-01-01';

SELECT * FROM profitwisemastervariant;

SELECT COUNT(*) FROM shopifyorderlineitem;


SELECT * FROM exchangerate;

SELECT * FROM calendar_table;

