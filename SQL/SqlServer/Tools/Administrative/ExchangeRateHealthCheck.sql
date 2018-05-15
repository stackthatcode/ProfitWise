USE ProfitWise;
GO

DECLARE @NumberOfDays int;
DECLARE @NumberOfCurrencies int;
DECLARE @ActualExchangeRateEntries int;

SELECT @NumberOfDays = DATEDIFF(day, MIN(Date), MAX(Date)) + 1 FROM exchangerate;

SELECT @NumberOfCurrencies = COUNT(*) FROM currency;

SELECT @ActualExchangeRateEntries = COUNT(*) FROM exchangerate;

DECLARE @ExpectedExchangeRateEntries int = @NumberOfDays * @NumberOfCurrencies * @NumberOfCurrencies;


SELECT FORMATMESSAGE(
		'Number of Days = %i / Expected # of Entries = %i / Actual # of Entries = %i',
		@NumberOfDays, @ExpectedExchangeRateEntries, @ActualExchangeRateEntries);


SELECT SourceCurrencyId, COUNT(*), MIN(Date), MAX(Date) 
FROM exchangerate
GROUP BY SourceCurrencyId;

SELECT Date, SUM(Rate), COUNT(*) FROM dbo.exchangerate WHERE Date >= '2018-03-01' GROUP BY Date
ORDER BY Date;
