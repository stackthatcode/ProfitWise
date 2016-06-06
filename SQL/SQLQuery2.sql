
SELECT COUNT(*) FROM OrderSkuHistory

SELECT TOP 10 * FROM OrderSkuHistory


PRINT NOW()

DECLARE @counter int = 1

WHILE @counter < (100)
BEGIN

SELECT * FROM OrderSkuHistory WHERE LineId > 1300000 AND LineId < 1300043
	SET @counter = @counter + 1

END


