USE ProfitWise;
GO


ALTER TABLE profitwiseprofitreportentry ADD UnitPrice decimal NULL;

ALTER TABLE profitwiseprofitreportentry ADD UnitCoGS decimal NULL;


ALTER TABLE dbo.shopifyorderlineitem
ALTER COLUMN Quantity DECIMAL(18,2) 

DROP FUNCTION IF EXISTS dbo.SaveDivide
GO
CREATE FUNCTION dbo.SaveDivide (@Dividend decimal(18, 4), @Divisor decimal(18, 4))
RETURNS decimal(18, 4)
AS
BEGIN
	RETURN CASE WHEN @Divisor = 0 THEN 0 ELSE @Dividend / @Divisor END
END
GO


DROP FUNCTION IF EXISTS dbo.SaveDivideAlt
GO
CREATE FUNCTION dbo.SaveDivideAlt (@Dividend decimal(18, 2), @Divisor decimal(18, 4), @AlternateValue decimal(18, 4))
RETURNS decimal(18, 4)
AS
BEGIN
	RETURN CASE WHEN @Divisor = 0 THEN @AlternateValue ELSE @Dividend / @Divisor END
END
GO
