USE ProfitWise
GO


SELECT * INTO calendar_table_OLD FROM calendar_table;

DELETE FROM calendar_table;


ALTER TABLE dbo.calendar_table ALTER COLUMN w int NOT NULL


