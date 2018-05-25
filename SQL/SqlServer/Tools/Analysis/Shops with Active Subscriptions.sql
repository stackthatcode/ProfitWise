USE ProfitWise;
GO


SELECT * FROM vw_profitwiseshop 
WHERE IsAccessTokenValid = 1 
AND IsProfitWiseInstalled = 1 
AND LastBillingStatus = 3


