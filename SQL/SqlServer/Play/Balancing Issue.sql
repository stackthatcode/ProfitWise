USE ProfitWise
GO


SELECT * FROM profitreportentry(100001);

exec sp_executesql 
	N'SELECT SUM(t3.NetSales) As TotalRevenue,
    SUM(t3.Quantity) AS TotalQuantitySold,
    COUNT(DISTINCT(t3.ShopifyOrderId)) AS TotalOrders,
	SUM(t3.CoGS) AS TotalCogs, 
    SUM(t3.NetSales) - SUM(t3.CoGS) AS TotalProfit,
    CASE WHEN SUM(t3.NetSales) = 0 THEN 0 
        ELSE 100.0 - (100.0 * SUM(t3.CoGS) / SUM(t3.NetSales)) END AS AverageMargin 
	FROM profitquerystub(@PwShopId) t1
	INNER JOIN variant(@PwShopId) t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId 
	INNER JOIN profitreportentryprocessed(@PwShopId, @UseDefaultMargin, @DefaultCogsPercent, @MinPaymentStatus) t3
		ON t2.PwProductId = t3.PwProductId 
            AND t2.PwVariantId = t3.PwVariantId
            AND t3.EntryDate >= @StartDate
            AND t3.EntryDate <= @EndDate             
    WHERE t1.PwReportId = @PwReportId ',N'@DefaultCogsPercent decimal(2,2),@EndDate datetime,@MinPaymentStatus int,@PwReportId bigint,@PwShopId bigint,@StartDate datetime,@UseDefaultMargin bit',@DefaultCogsPercent=0.80,@EndDate='2017-12-31 00:00:00',@MinPaymentStatus=1,@PwReportId=99951,@PwShopId=100001,@StartDate='2014-01-01 00:00:00',@UseDefaultMargin=1




SELECT * FROM profitquerystub(100001) WHERE PwReportId = 99952

SELECT * FROM variant(100001) WHERE PwMasterVariantId = 288;

SELECT * FROM profitreportentryprocessed(100001, 1, 0, 1) WHERE PwProductId = 47

SELECT ShopifyOrderId, NetSales FROM profitreportentry(100001) WHERE EntryType = 3 AND NetSales > 0

SELECT SUM(NetSales) FROM profitreportentry(100001) WHERE ShopifyOrderId = 3440132741

SELECT COUNT(DISTINCT(ShopifyOrderId)), SUM(Quantity) FROM profitreportentry(100001) WHERE PwProductId = 47

SELECT * FROM variant(100001) WHERE PwProductId = 47

SELECT * FROM mastervariant(100001) WHERE PwMasterVariantId = 288;

SELECT * FROM profitreportentry(100001) WHERE ShopifyOrderId = 4844493906;

SELECT SUM(t1.NetSales)
FROM profitreportentry(100001) t1
	INNER JOIN ordertable(100001) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE t1.EntryType = 3 AND t2.Cancelled = 1

SELECT SUM(t1.NetSales)
FROM profitreportentry(100001) t1
	INNER JOIN ordertable(100001) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE t1.EntryType = 3 AND t2.Cancelled = 0

SELECT * FROM profitreportentry(100001) WHERE EntryType = 2

SELECT ShopifyOrderId FROM ordertable(100001);


SELECT ShopifyOrderId, SUM(NetSales) 
FROM profitreportentry(100001) 
WHERE PwProductId = 47 
GROUP BY ShopifyOrderId 
ORDER BY ShopifyOrderId DESC;

4486468882 => Net Sales(PW) == 14.26
4486468882 => Net Sales(SHOP) == 19.99

SELECT SUM(NetSales) FROM profitreportentry(100001) WHERE ShopifyOrderId = 4486468882;



exec sp_executesql 
	N'SELECT SUM(t3.NetSales) As TotalRevenue,
    SUM(t3.Quantity) AS TotalQuantitySold,
    COUNT(DISTINCT(t3.ShopifyOrderId)) AS TotalOrders,
	SUM(t3.CoGS) AS TotalCogs, 
    SUM(t3.NetSales) - SUM(t3.CoGS) AS TotalProfit,
    CASE WHEN SUM(t3.NetSales) = 0 THEN 0 
        ELSE 100.0 - (100.0 * SUM(t3.CoGS) / SUM(t3.NetSales)) END AS AverageMargin  
	FROM profitreportentryprocessed(@PwShopId, @UseDefaultMargin, @DefaultCogsPercent, @MinPaymentStatus) t3
	WHERE t3.EntryDate >= @StartDate AND t3.EntryDate <= @EndDate 
    AND t3.EntryType = @AdjustmentEntry',
	N'@AdjustmentEntry int,@DefaultCogsPercent decimal(2,2),@EndDate datetime,@MinPaymentStatus int,@PwShopId bigint,@StartDate datetime,@UseDefaultMargin bit',
	@AdjustmentEntry=3,@DefaultCogsPercent=0.80,@EndDate='2017-12-31 00:00:00',@MinPaymentStatus=1,@PwShopId=100001,@StartDate='2014-01-01 00:00:00',@UseDefaultMargin=1



SELECT * FROM profitreportentry(100001) WHERE EntryType = 3;

SELECT SUM(BalancingCorrection) FROM ordertable(100001);



exec sp_executesql 
	N'SELECT COUNT(DISTINCT(t3.ShopifyOrderId)) 
    FROM profitquerystub(@PwShopId) t1
	    INNER JOIN variant(@PwShopId) t2
		    ON t1.PwMasterVariantId = t2.PwMasterVariantId 
	    INNER JOIN profitreportentryprocessed(@PwShopId, @UseDefaultMargin, @DefaultCogsPercent, @MinPaymentStatus) t3
		    ON t2.PwProductId = t3.PwProductId 
			    AND t2.PwVariantId = t3.PwVariantId
			    AND t3.EntryDate >= @StartDate
			    AND t3.EntryDate <= @EndDate 
                AND t3.EntryType = 1 
    WHERE t1.PwReportId = @PwReportId;',N'@DefaultCogsPercent decimal(2,2),@EndDate datetime,@MinPaymentStatus int,@PwReportId bigint,@PwShopId bigint,@StartDate datetime,@UseDefaultMargin bit',@DefaultCogsPercent=0.80,@EndDate='2017-12-31 00:00:00',@MinPaymentStatus=1,@PwReportId=99951,@PwShopId=100001,@StartDate='2014-01-01 00:00:00',@UseDefaultMargin=1



SELECT SUM(Amount) FROM orderadjustment(100001) WHERE ShopifyOrderId IN (
	SELECT ShopifyOrderId FROM ordertable(100001) WHERE Cancelled = 1 AND FinancialStatus = 7
);

SELECT * FROM ordertable(100001) WHERE Cancelled = 1


SELECT SUM(t1.Amount), t2.Cancelled, t2.FinancialStatus
FROM orderadjustment(100001) t1
	INNER JOIN ordertable(100001) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
	WHERE 
GROUP BY t2.Cancelled, t2.FinancialStatus
ORDER BY t2.Cancelled, t2.FinancialStatus

SELECT SUM(t1.Amount) FROM orderadjustment(100001) t1 



SELECT  t2.Cancelled, t2.FinancialStatus, t1.PaymentStatus, COUNT(DISTINCT(t1.ShopifyOrderId)), SUM(NetSales)
FROM profitreportentry(100001) t1
	INNER JOIN ordertable(100001) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE EntryDate >= '2016-05-01' AND EntryDate <= '2016-05-30' AND EntryType <> 4
GROUP BY t2.Cancelled, t2.FinancialStatus, t1.PaymentStatus



SELECT * FROM profitreportentry(100001) WHERE EntryType = 2

SELECT * FROM ordertable(100001) WHERE BalancingCorrection > 0;

SELECT * FROM profitreportentry(100001) t1 
WHERE EntryDate >= '2016-08-01' AND EntryDate <= '2016-08-31'
AND PwProductId = 4




SELECT * FROM profitreportentry(100001) WHERE EntryDate >= '2016-10-01' AND EntryDate <= '2016-10-31' AND PwProductId = 4


SELECT t2.ShopifyOrderId, SUM(NetSales)
FROM profitreportentry(100001) t1
	INNER JOIN ordertable(100001) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE EntryDate >= '2016-11-01' AND EntryDate <= '2016-11-30'
AND t2.Cancelled = 0
AND t2.FinancialStatus = 5
GROUP BY t2.ShopifyOrderId
ORDER BY t2.ShopifyOrderId


	
SELECT SUM(NetSales) FROM profitreportentry(100001) WHERE EntryDate >= '2017-05-03' AND EntryDate <= '2017-05-03';

SELECT ShopifyOrderId FROM profitreportentry(100001) WHERE EntryDate >= '2017-05-03' AND EntryDate <= '2017-05-03';



SELECT  t2.Cancelled, t2.FinancialStatus, t1.PaymentStatus, COUNT(DISTINCT(t1.OrderCountOrderId)), SUM(NetSales)
FROM profitreportentry(100001) t1 INNER JOIN ordertable(100001) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE EntryDate >= '2017-05-02' AND EntryDate <= '2017-05-02'
GROUP BY t2.Cancelled, t2.FinancialStatus, t1.PaymentStatus


SELECT EntryType, SUM(NetSales) FROM profitreportentryprocessed(100001, 1, 0.80, 1) 
WHERE EntryDate >= '2017-05-02' AND EntryDate <= '2017-05-02'
GROUP BY EntryType;

SELECT *  FROM profitreportentryprocessed(100001, 1, 0.80, 1) 
WHERE EntryDate >= '2017-05-02' AND EntryDate <= '2017-05-02' ORDER BY ShopifyOrderId;

SELECT * FROM profitreportentry(100006) WHERE PwProductID = 0;


SELECT * FROM orderrefund(100001) WHERE PwProductId = 0;



SELECT COUNT(DISTINCT(OrderCountOrderId)) FROM profitreportentry(100001) WHERE EntryType = 1;



