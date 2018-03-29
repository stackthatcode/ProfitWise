
USE ProfitWise;
GO


SELECT * FROM currency;

SELECT * FROM systemstate;

SELECT MAX(Date) FROM exchangerate;


SELECT t1.Date, t1.Rate, t2.Date, t2.Rate
FROM exchangerate t1 
	INNER JOIN exchangerate_BACKUPWITHAED t2
		ON t1.Date = t2.Date
		AND t1.SourceCurrencyId = t2.SourceCurrencyId
		AND t1.DestinationCurrencyId = t2.DestinationCurrencyId
WHERE t1.Rate <> t2.Rate;


SELECT * FROM exchangerate ORDER BY Date, SourceCurrencyId, DestinationCurrencyId;

SELECT * FROM exchangerate_BACKUP WHERE Date = '2006-01-01' ORDER BY SourceCurrencyId, DestinationCurrencyId;

SELECT * INTO exchangerate_BACKUPWITHAED FROM exchangerate;


/*
DELETE FROM exchangerate WHERE Date >= '2008-01-01';

UPDATE systemstate SET ExchangeRateLastDate = NULL '2018-04-26';
*/


/*
INSERT INTO exchangerate 
SELECT SourceCurrencyId, DestinationCurrencyId, DATEADD(day, 1, [Date]), Rate
FROM exchangerate WHERE Date = '2008-01-04';

UPDATE systemstate SET ExchangeRateLastDate = '2008-01-05';
*/




SELECT * FROM profitwiseshop t1
	INNER JOIN profitwisemastervariant t2
		ON t1.PwShopId = t2.PwShopId
WHERE t1.CurrencyId <> t2.CogsCurrencyId;


SELECT COUNT(*)
FROM profitwiseshop t1
	INNER JOIN profitwisemastervariant t2
		ON t1.PwShopId = t2.PwShopId
WHERE t1.CurrencyId <> t2.CogsCurrencyId;

SELECT COUNT(*)
FROM profitwiseshop t1
	INNER JOIN  profitwisemastervariantcogsdetail t2
		ON t1.PwShopId = t2.PwShopId
WHERE t1.CurrencyId <> t2.CogsCurrencyId


SELECT COUNT(*)
FROM profitwiseshop t1
	INNER JOIN profitwisemastervariant t2
		ON t1.PwShopId = t2.PwShopId
WHERE t1.CurrencyId = t2.CogsCurrencyId;


SELECT COUNT(*)
FROM profitwiseshop t1
	INNER JOIN  profitwisemastervariantcogsdetail t2
		ON t1.PwShopId = t2.PwShopId
WHERE t1.CurrencyId <> t2.CogsCurrencyId;

SELECT COUNT(*)
FROM profitwiseshop t1
	INNER JOIN  profitwisemastervariantcogsdetail t2
		ON t1.PwShopId = t2.PwShopId
WHERE t1.CurrencyId = t2.CogsCurrencyId;


-- WHERE PwShopId = 100077


SELECT YEAR(OrderDate), COUNT(DISTINCT(PwShopId)) 
FROM shopifyorder GROUP BY YEAR(OrderDate);


SELECT * FROM currency;

SELECT * FROM dbo.exchangerate WHERE Date = '2006-01-01'
ORDER BY SourceCurrencyId, DestinationCurrencyId;

SELECT MAX(Date) FROM dbo.exchangerate WHERE SourceCurrencyId = 8;

SELECT * FROM dbo.exchangerate WHERE SourceCurrencyId = 8;


WITH CTE (Date, SourceCurrencyId, Cnt) AS
(
	SELECT [Date], SourceCurrencyId, COUNT(*) 
	FROM dbo.exchangerate WHERE [Date] <= '2012-11-11'
	GROUP BY [Date], SourceCurrencyId
)
SELECT * FROM CTE WHERE Cnt < 8
ORDER BY [Date], SourceCurrencyId

DELETE FROM dbo.exchangerate;

