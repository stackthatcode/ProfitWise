USE ProfitWise;
GO



-- ALTER TABLE profitwiseshop DROP COLUMN DisabledCode

IF COL_LENGTH('dbo.profitwiseshop', 'DisabledCode') IS NULL
BEGIN
	ALTER TABLE profitwiseshop ADD DisabledCode int NULL;
END
GO
