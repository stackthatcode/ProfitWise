
USE ProfitWise;
GO


SELECT PwShopId, Domain
FROM [vw_profitwiseshop] 
WHERE LastBillingStatus = 3 AND IsAccessTokenValid = 1 AND IsProfitWiseInstalled = 1;


SELECT DATEPART(year, EntryDate) AS [Year], DATEPART(month, EntryDate) AS [Month], SUM(NetSales)
FROM dbo.profitreportentry(100035)
GROUP BY DATEPART(year, EntryDate),DATEPART(month, EntryDate)
ORDER BY DATEPART(year, EntryDate),DATEPART(month, EntryDate)


