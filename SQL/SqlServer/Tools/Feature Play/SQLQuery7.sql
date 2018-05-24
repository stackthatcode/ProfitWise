exec sp_executesql 
	N'SELECT 
		t1.Vendor, 
		t1.ProductType,
		t1.ProductTitle,
		t1.VariantTitle,
		t1.Sku, 

		SUM(t3.Quantity) AS TotalQuantitySold, 
		COUNT(DISTINCT(t3.OrderCountOrderId)) AS TotalOrders, 
		SUM(t3.NetSales) As TotalRevenue, 
		SUM(t3.CoGS) AS TotalCogs,	 
		SUM(t3.NetSales) - SUM(t3.CoGS) AS TotalProfit, 
		100.0 - dbo.SaveDivideAlt(100.0 * SUM(t3.CoGS), SUM(t3.NetSales), 100.0) AS AverageMarginPercent, 
		dbo.SaveDivide((SUM(NetSales) - SUM(CoGS)), SUM(Quantity)) AS AverageMarginPerUnitSold,

		dbo.SaveDivide(SUM(Quantity * UnitPrice), SUM(Quantity)) AS UnitPriceAverage, 
		dbo.SaveDivide(SUM(Quantity * UnitCoGS), SUM(Quantity)) AS UnitCogsAverage,

		t4.CurrentUnitPrice,
		t4.UnitCogsByDate AS CurrentUnitCogs,
		t4.CurrentUnitPrice - t4.UnitCogsByDate AS CurrentMargin,

		t4.StockedDirectly

	FROM profitquerystub(@PwShopId) t1
		INNER JOIN variant(@PwShopId) t2
			ON t1.PwMasterVariantId = t2.PwMasterVariantId 
		INNER JOIN profitreportentryprocessed(@PwShopId, @UseDefaultMargin, @DefaultCogsPercent, @MinPaymentStatus) t3
			ON t2.PwProductId = t3.PwProductId 
				AND t2.PwVariantId = t3.PwVariantId
				AND t3.EntryDate >= @StartDate
				AND t3.EntryDate <= @EndDate             
		INNER JOIN costofgoodsbydate(@PwShopId, DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)) t4
			ON t2.PwVariantId = t4.PwVariantId		
		WHERE t1.PwReportId = @PwReportId 
		GROUP BY t1.Vendor, t1.ProductType, t1.ProductTitle, t1.VariantTitle, t1.Sku, 
			t4.CurrentUnitPrice, t4.UnitCogsByDate, t4.CurrentUnitPrice - t4.UnitCogsByDate, 
			t4.StockedDirectly, t4.Exclude',

	N'@DefaultCogsPercent decimal(2,2),@EndDate datetime,@MinPaymentStatus int,@PwReportId bigint,@PwShopId bigint,@StartDate datetime,@UseDefaultMargin bit'
		
	,@DefaultCogsPercent=0.80
	,@EndDate='2018-05-31 00:00:00'
	,@MinPaymentStatus=1
	,@PwReportId=1
	,@PwShopId=100001
	,@StartDate='2018-05-01 00:00:00'
	,@UseDefaultMargin=1


--		SELECT GETDATE()