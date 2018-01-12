USE ProfitWise
GO

IF COL_LENGTH('dbo.profitwiseshop', 'FailedAuthorizationCount') IS NULL
BEGIN
	ALTER TABLE dbo.profitwiseshop
	ADD FailedAuthorizationCount int NULL;
END
GO

UPDATE dbo.profitwiseshop SET FailedAuthorizationCount = 0;

ALTER TABLE dbo.profitwiseshop ALTER COLUMN FailedAuthorizationCount int NOT NULL

-- PROD data fix to force Failed Authorization -> and ultimately Shop Uninstall


UPDATE dbo.profitwiseshop SET IsAccessTokenValid = 1
WHERE IsAccessTokenValid = 0 AND IsProfitWiseInstalled = 1



