
USE ProfitWise
GO

SELECT COUNT(*) FROM OrderSkuHistory


DELETE FROM OrderSkuHistory


DECLARE @counter int = 1

WHILE @counter < (50000 * 1000)

BEGIN 
	INSERT INTO OrderSkuHistory (OrderNumber, ProductSku, Price, COGS) 	
	VALUES ( 'ABCDEFDFK', 'TESTSKU', 20, 10 )
	
	SET @counter = @counter + 1
END



DECLARE @counter int = 1

WHILE @counter < (10)
BEGIN

SELECT * FROM OrderSkuHistory WHERE LineId > 4300000 AND LineId <4300043
	SET @counter = @counter + 1

END

