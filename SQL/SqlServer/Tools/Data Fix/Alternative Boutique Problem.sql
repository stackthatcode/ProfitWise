USE ProfitWise;
GO


SELECT * FROM variant(100056) WHERE SKU LIKE '%19781%' OR SKU LIKE '%19881%'

SELECT * FROM variant(100056) WHERE PwProductId IN
( SELECT PwProductId FROM product(100056) WHERE Title LIKE '%octopus%' );

SELECT * FROM shop(100056);


