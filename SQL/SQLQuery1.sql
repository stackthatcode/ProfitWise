

SELECT * FROM OrderSkuHistory



CREATE CLUSTERED INDEX i1 ON OrderSkuHistory (LineId);  



DECLARE @counter int = 1

WHILE @counter < (50000 * 1000)

BEGIN 
	INSERT INTO OrderSkuHistory (OrderNumber, ProductSku, Price, COGS) 	
	VALUES ( 'ABCDEFDFK', 'TESTSKU', 20, 10 )
	
	SET @counter = @counter + 1
END

