USE ProfitWise;
GO

ALTER TABLE shopifyorderlineitem
ALTER COLUMN UnitCogs decimal(15, 6);

ALTER TABLE profitwiseprofitreportentry
ALTER COLUMN CoGS decimal(15, 6);

